using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Pandora.Models;
using Pandora.Repositories;

namespace Pandora.Services
{
    public interface IConsolidatedBackupsService
    {
        IQueryable<ConsolidatedValidationsData> GetAllAsNoTracking();
        IQueryable<ConsolidatedValidationsData> GetAll();
        Task<List<ConsolidatedValidationsData>> GetAllListAsync();
        Task<ConsolidatedValidationsData> GetRecordByIdAsync(int? id);
        Task<ConsolidatedValidationsData> FindRecordByIdAsync(int? id);
        IQueryable<ConsolidatedValidationsData> Filter(IQueryable<ConsolidatedValidationsData> backupRecords, string filter, string filterRule, string searchOption);
        
        // Business Logic Methods
        (int DatasetEntry, int TotalDatasets, double PercentageBackedup, double PercentageValidated) GetIndexMetrics(IQueryable<ConsolidatedValidationsData> filteredRecords);
        Task ProcessEditAsync(ConsolidatedValidationsData updatedData, string currentUser);
        Task<(bool IsSuccess, int ValidatedCount, string ErrorMessage)> ProcessBulkValidationAsync(IFormFile upload, string currentUser, string currentDirectory);
        Task<(bool IsSuccess, string ErrorMessage)> ProcessUnclaimAsync(int id, string currentUser);
        Task ProcessInvalidateAsync(int id, string currentUser);
        byte[] GenerateExportCsv(string filter, string filterRule, string searchOption);
    }

    public class ConsolidatedBackupsService : IConsolidatedBackupsService
    {
        private readonly IConsolidatedBackupsRepository _repository;

        public ConsolidatedBackupsService(IConsolidatedBackupsRepository repository)
        {
            _repository = repository;
        }

        public IQueryable<ConsolidatedValidationsData> GetAllAsNoTracking() => _repository.GetAllAsNoTracking();
        public IQueryable<ConsolidatedValidationsData> GetAll() => _repository.GetAll();
        public async Task<List<ConsolidatedValidationsData>> GetAllListAsync() => await _repository.GetAllListAsync();
        public async Task<ConsolidatedValidationsData> GetRecordByIdAsync(int? id) => await _repository.GetByIdAsync(id);
        public async Task<ConsolidatedValidationsData> FindRecordByIdAsync(int? id) => await _repository.FindByIdAsync(id);

        public (int DatasetEntry, int TotalDatasets, double PercentageBackedup, double PercentageValidated) GetIndexMetrics(IQueryable<ConsolidatedValidationsData> filteredRecords)
        {
            int datasetEntry = filteredRecords.Count();
            int totalDatasets = filteredRecords.Sum(a => a.MigrateNoOfDatasets) + filteredRecords.Sum(a => a.TapeDSNoOfDatasets);
            
            var noOfBackedupItems = filteredRecords.Where(m => m.IsBackup == "YES").Count();
            var percentageOfBackedupItems = Math.Round((double)noOfBackedupItems / (double)datasetEntry * 100, 1);
            double finalPercentageBackedup = Double.IsNaN(percentageOfBackedupItems) ? 0 : percentageOfBackedupItems;

            var noOfValidatedItems = filteredRecords.Where(m => m.Validated == true).Count();
            var percentageOfValidatedItems = Math.Round((double)noOfValidatedItems / (double)datasetEntry * 100, 1);
            double finalPercentageValidated = Double.IsNaN(percentageOfValidatedItems) ? 0 : percentageOfValidatedItems;

            return (datasetEntry, totalDatasets, finalPercentageBackedup, finalPercentageValidated);
        }

        public async Task ProcessEditAsync(ConsolidatedValidationsData updatedData, string currentUser)
        {
            var record = await _repository.GetByIdAsync(updatedData.Id);
            
            record.ApplicationOwner = updatedData.ApplicationOwner;
            record.DataOwner = updatedData.DataOwner;
            record.BackupSettingsValid = updatedData.BackupSettingsValid;
            record.DataRequiredOrRedundant = updatedData.DataRequiredOrRedundant;
            record.RetentionSettingValid = updatedData.RetentionSettingValid;
            record.Tower = updatedData.Tower;
            record.Service = updatedData.Service;
            record.LastUpdatedBy = currentUser;

            if (string.IsNullOrEmpty(record.ApplicationOwner) &&
                string.IsNullOrEmpty(record.DataOwner) &&
                string.IsNullOrEmpty(record.BackupSettingsValid) &&
                string.IsNullOrEmpty(record.DataRequiredOrRedundant) &&
                string.IsNullOrEmpty(record.RetentionSettingValid))
            {
                record.Validated = false;
                record.Status = "Under Investigation";
            }
            else
            {
                record.Validated = true;
                record.Status = "Validated";
            }
            
            record.ValidatedBy = currentUser;
            await _repository.UpdateAsync(record);
        }

        public async Task<(bool IsSuccess, int ValidatedCount, string ErrorMessage)> ProcessBulkValidationAsync(IFormFile upload, string currentUser, string currentDirectory)
        {
            string path = Path.Combine(currentDirectory, "wwwroot" + "/UploadValidations/Validations/" + upload.FileName);
            
            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }

                var consolidatedBackups = await _repository.GetAllListAsync();
                var workbook = new XLWorkbook(path);
                int counter = 0;

                if (workbook.Worksheets.Count() > 0)
                {
                    var worksheet = workbook.Worksheet(1);
                    int totalRows = worksheet.RowsUsed().Count();
                    int totalColumns = worksheet.ColumnsUsed().Count();

                    if (totalColumns == 28 || totalColumns == 29)
                    {
                        for (int i = 2; i <= totalRows; i++)
                        {
                            var dataOwner = Convert.ToString(worksheet.Cell(i, 20).Value);
                            var applicationOwner = Convert.ToString(worksheet.Cell(i, 21).Value);
                            var backupSettingsValid = Convert.ToString(worksheet.Cell(i, 24).Value).ToUpper();
                            var dataRequiredOrRedundant = Convert.ToString(worksheet.Cell(i, 25).Value).ToUpper();
                            var retentionSettingValid = Convert.ToString(worksheet.Cell(i, 26).Value).ToUpper();
                            var service = Convert.ToString(worksheet.Cell(i, 27).Value).ToUpper();
                            var tower = Convert.ToString(worksheet.Cell(i, 28).Value).ToUpper();

                            if (string.IsNullOrEmpty(dataOwner) && string.IsNullOrEmpty(applicationOwner) &&
                                (string.IsNullOrEmpty(backupSettingsValid) || backupSettingsValid == "REDUNDANT") &&
                                (string.IsNullOrEmpty(dataRequiredOrRedundant) || dataRequiredOrRedundant == "REDUNDANT") &&
                                string.IsNullOrEmpty(retentionSettingValid) && string.IsNullOrEmpty(service) && string.IsNullOrEmpty(tower))
                            {
                                var dsnhlq = Convert.ToString(worksheet.Cell(i, 2).Value);
                                var consolidatedBackup = consolidatedBackups.Where(a => a.DSNHLQ.Equals(dsnhlq)).FirstOrDefault();
                                
                                if (consolidatedBackup != null && consolidatedBackup.Status != "Validated")
                                {
                                    consolidatedBackup.Validated = true;
                                    consolidatedBackup.ApplicationOwner = applicationOwner;
                                    consolidatedBackup.DataOwner = dataOwner;
                                    consolidatedBackup.BackupSettingsValid = backupSettingsValid;
                                    consolidatedBackup.DataRequiredOrRedundant = dataRequiredOrRedundant;
                                    consolidatedBackup.RetentionSettingValid = retentionSettingValid;
                                    consolidatedBackup.Service = service;
                                    consolidatedBackup.Tower = tower;
                                    consolidatedBackup.Status = "Validated";

                                    await _repository.UpdateAsync(consolidatedBackup);
                                    counter++;
                                }
                            }
                        }

                        if (File.Exists(path)) File.Delete(path);

                        if (counter == 0) return (false, 0, "Error! No items to validate.");
                        
                        return (true, counter, null);
                    }
                    else
                    {
                        if (File.Exists(path)) File.Delete(path);
                        return (false, 0, "Error! Please check file columns or file data.");
                    }
                }
                
                if (File.Exists(path)) File.Delete(path);
                return (false, 0, "Error! Worksheet empty.");
            }
            catch (Exception)
            {
                if (File.Exists(path)) File.Delete(path);
                return (false, 0, "Error uploading file!");
            }
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ProcessUnclaimAsync(int id, string currentUser)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record == null) return (false, "Not Found");

            if (string.IsNullOrEmpty(record.ApplicationOwner) &&
                string.IsNullOrEmpty(record.DataOwner) &&
                string.IsNullOrEmpty(record.BackupSettingsValid) &&
                string.IsNullOrEmpty(record.DataRequiredOrRedundant) &&
                string.IsNullOrEmpty(record.RetentionSettingValid))
            {
                record.Status = "Under Investigation";
                record.ValidatedBy = currentUser;
                record.LastUpdatedBy = currentUser;
                record.Validated = false;

                await _repository.UpdateAsync(record);
                return (true, null);
            }
            
            return (false, "Unable to unclaim! Please try again.");
        }

        public async Task ProcessInvalidateAsync(int id, string currentUser)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record != null)
            {
                record.Status = "Under Investigation";
                record.ApplicationOwner = "";
                record.DataOwner = "";
                record.BackupSettingsValid = "";
                record.DataRequiredOrRedundant = "";
                record.RetentionSettingValid = "";
                record.Tower = "";
                record.Service = "";
                record.ValidatedBy = "";
                record.Validated = false;

                await _repository.UpdateAsync(record);
            }
        }

        public byte[] GenerateExportCsv(string filter, string filterRule, string searchOption)
        {
            var backupRecords = _repository.GetAllAsNoTracking();
            backupRecords = Filter(backupRecords, filter, filterRule, searchOption);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("BatchCode,Service,Tower,Type,HLQ1,HLQ2,ManagementClass,IsBackup,CreatedYear,Disk NoofDatasets,Disk Size,BackupNoofDatasets,BackupSize,MigrateNoOfDatasets,MigrateSize,TapeDSNoOfDatasets,TapeDSSize,Datatype,ApplicationOwner,DataOwner,ValidatedBy,BackupSettingValid,RetentionSettingValid,DataRequiredOrRedundant,Status,LastUpdatedBy,DSNHLQ,Validated?");

            foreach (var record in backupRecords)
            {
                stringBuilder.AppendLine($"{record.BatchCode},{record.Service},{record.Tower},{record.Type},{record.HLQ1},{record.HLQ2},{record.ManagementClass},{record.IsBackup},{record.CreatedYear},{record.DiskNoOfDatasets},{record.DiskSize},{record.BackupNoOfDatasets},{record.BackupSize},{record.MigrateNoOfDatasets},{record.MigrateSize},{record.TapeDSNoOfDatasets},{record.TapeDSSize},{record.Datatype},{record.ApplicationOwner},{record.DataOwner},{record.ValidatedBy},{record.BackupSettingsValid},{record.RetentionSettingValid},{record.DataRequiredOrRedundant},{record.Status},{record.LastUpdatedBy},{record.DSNHLQ},{record.Validated}");
            }

            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }

        public IQueryable<ConsolidatedValidationsData> Filter(IQueryable<ConsolidatedValidationsData> backupRecords, string filter, string filterRule, string searchOption)
        {
            if (!string.IsNullOrEmpty(filterRule))
            {
                if (filterRule == "DatasetEntriesBackedUp")
                {
                    backupRecords = backupRecords.Where(m => m.IsBackup == "YES");
                }
                else if (filterRule == "DatasetEntriesValidated")
                {
                    backupRecords = backupRecords.Where(m => m.Status == "Validated");
                }
            }

            if (!string.IsNullOrWhiteSpace(searchOption))
            {
                var filterNormalize = "";
                if (!string.IsNullOrEmpty(filter))
                {
                    filterNormalize = filter.ToUpper().Trim();
                }

                if (searchOption == "BatchCode")
                    backupRecords = backupRecords.Where(s => s.BatchCode.ToUpper().Equals(filterNormalize));
                if (searchOption == "HLQ1")
                    backupRecords = backupRecords.Where(s => s.HLQ1.ToUpper().Contains(filterNormalize));
                if (searchOption == "Tower")
                    backupRecords = backupRecords.Where(s => s.Tower.ToUpper().Contains(filterNormalize));
                if (searchOption == "Type")
                    backupRecords = backupRecords.Where(s => s.Type.ToUpper().Contains(filterNormalize));
                if (searchOption == "Service")
                    backupRecords = backupRecords.Where(s => s.Service.ToUpper().Contains(filterNormalize));
                if (searchOption == "ManagementClass")
                    backupRecords = backupRecords.Where(s => s.ManagementClass.ToUpper().Contains(filterNormalize));
                if (searchOption == "CreatedYear")
                    backupRecords = backupRecords.Where(s => s.CreatedYear.ToString().Equals(filterNormalize));
                if (searchOption == "NoOfDatasets")
                    backupRecords = backupRecords.Where(s => s.MigrateNoOfDatasets.ToString().Equals(filterNormalize) || s.TapeDSNoOfDatasets.ToString().Equals(filterNormalize));
                if (searchOption == "Size")
                    backupRecords = backupRecords.Where(s => s.Size.ToString().Equals(filterNormalize) || s.TapeDSSize.ToString().Equals(filterNormalize));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var filterNormalize = filter.Trim();
                    backupRecords = backupRecords.Where(s => s.BatchCode.ToUpper().Contains(filterNormalize.ToUpper()) ||
                                                             s.HLQ1.ToUpper().Contains(filterNormalize.ToUpper()) ||
                                                             s.Tower.ToUpper().Contains(filterNormalize.ToUpper()) ||
                                                             s.Type.ToUpper().Contains(filterNormalize.ToUpper()) ||
                                                             s.Service.ToUpper().Contains(filterNormalize.ToUpper()) ||
                                                             s.ManagementClass.ToUpper().Contains(filterNormalize.ToUpper()) ||
                                                             s.CreatedYear.ToString().Equals(filterNormalize) ||
                                                             s.MigrateNoOfDatasets.ToString().Equals(filterNormalize) ||
                                                             s.TapeDSNoOfDatasets.ToString().Equals(filterNormalize) ||
                                                             s.Size.ToString().Equals(filterNormalize) ||
                                                             s.TapeDSSize.ToString().Equals(filterNormalize) ||
                                                             s.Validated.ToString().Equals(filterNormalize));
                }
            }

            return backupRecords;
        }
    }
}