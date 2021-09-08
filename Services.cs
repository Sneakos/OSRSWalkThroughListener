using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using TransferObjects;

namespace OSRSWalkThroughListener
{
    public class Services
    {
        public Services()
        {
            SetAssetsFolder();
        }

        #region Services

        public ForcePushTasksResponse ExecuteService(ForcePushTasks request)
        {
            try
            {
                SaveTasks(request.Tasks, "Master");

                return new ForcePushTasksResponse
                {
                    Result = new ServiceResult
                    {
                        Result = true,
                        Data = "Force Pushed!"
                    }
                };
            }
            catch (Exception)
            {
                return new ForcePushTasksResponse
                {
                    Result = new ServiceResult
                    {
                        Result = false,
                        Data = "Force Pushed Failed!"
                    }
                };
            }
        }

        public GetMasterTaskListResponse ExecuteService(GetMasterTaskList request)
        {
            try
            {
                List<Task> tasks = LoadTasks("Master");

                return new GetMasterTaskListResponse
                {
                    Tasks = tasks
                };
            }
            catch (Exception)
            {
                return new GetMasterTaskListResponse();
            }
        }

        public GetBranchResponse ExecuteService(GetBranch request)
        {
            GetBranchResponse response = new();

            List<Task> tasks = LoadTasks(request.BranchName);

            response.Tasks = tasks;

            return response;
        }

        public PushToBranchResponse ExecuteService(PushToBranch request)
        {
            PushToBranchResponse response = new()
            {
                Result = new()
            };

            try
            {
                SaveTasks(request.Tasks, request.BranchName);

                response.Result.Result = true;
                response.Result.Data = "Pushed to " + request.BranchName;
            }
            catch (Exception ex)
            {
                response.Result.Result = false;
                response.Result.Data = "Push to " + request.BranchName + " failed: " + ex.Message;
            }

            return response;
        }

        #endregion Services

        #region Helpers

        public string ASSETS_FOLDER;

        private void SaveTasks(List<Task> tasks, string branchName)
        {
            string saveStr = JsonSerializer.Serialize(tasks);
            string savePath = ASSETS_FOLDER + branchName + @"\Tasks.data";

            saveStr = Base64Encode(saveStr);

            File.WriteAllText(savePath, saveStr);
        }

        private List<Task> LoadTasks(string branchName)
        {
            string loadPath = ASSETS_FOLDER + branchName + @"\Tasks.data";
            string loadStr = File.ReadAllText(loadPath);

            loadStr = Base64Decode(loadStr).Replace("SkillDelta", "ExperienceDelta");

            return JsonSerializer.Deserialize<List<Task>>(loadStr);
        }

        private void SetAssetsFolder()
        {
#if DEBUG
            ASSETS_FOLDER = @"..\..\..\Assets\"; ;
#else
            ASSETS_FOLDER = @".\Assets\"; ;
#endif
        }

        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        #endregion Helpers
    }
}
