﻿using EftPatchHelper.Interfaces;
using CG.Web.MegaApiClient;
using Spectre.Console;

namespace EftPatchHelper.Model
{
    public class MegaUpload : IFileUpload, IDisposable
    {
        public FileInfo UploadFileInfo { get; private set; }
        private MegaApiClient _client;
        private string _email;
        private string _password;
        private string _mfaKey;
        private INode _uploadFolder;
        private INode _uploadedFile;

        public string DisplayName { get; set; }
        public string ServiceName { get; set; }
        public string HubEntryText { get; set; }
        public bool AddHubEntry { get; }


        public MegaUpload(FileInfo file, string email, string password, string mfaKey = null)
        {
            _client = new MegaApiClient();
            UploadFileInfo = file;
            _email = email;
            _password = password;
            ServiceName = "Mega";
            DisplayName = $"{ServiceName} Upload";
            HubEntryText = $"Download from {ServiceName}";
            AddHubEntry = true;
        }

        private async Task<bool> CheckLoginStatus()
        {
            if (!_client.IsLoggedIn)
            {
                AnsiConsole.Markup("[blue]Logging into mega ... [/]");

                await _client.LoginAsync(_email, _password, _mfaKey);

                if (!_client.IsLoggedIn)
                {
                    AnsiConsole.MarkupLine("[red]failed[/]");
                    return false;
                }
                AnsiConsole.MarkupLine("[green]ok[/]");
            }

            return true;
        }

        public async Task<bool> SetUploadFolder(string folderName)
        {
            if (!await CheckLoginStatus())
            {
                return false;
            }

            AnsiConsole.Markup("[blue]Getting node ... [/]");
            var nodes = await _client.GetNodesAsync();

            var trashNode = nodes.SingleOrDefault(x => x.Type == NodeType.Trash);

            _uploadFolder = nodes.SingleOrDefault(x => x.Name == folderName && x.ParentId != trashNode.Id);

            bool nodeSet = _uploadFolder != null;

            AnsiConsole.MarkupLine(nodeSet != false ? "[green]node set[/]" : "[red]failed to set node[/]");

            return nodeSet;
        }

        public string GetLink()
        {
            return _client.GetDownloadLink(_uploadedFile).ToString();
        }

        public async Task<bool> UploadAsync(IProgress<double>? progress = null)
        {
            UploadFileInfo.Refresh();

            if (!UploadFileInfo.Exists) return false;

            if(!await CheckLoginStatus())
            {
                return false;
            }

            using var fileStream = UploadFileInfo.OpenRead();

            _uploadedFile = await _client.UploadAsync(fileStream, UploadFileInfo.Name, _uploadFolder, progress);

            return _uploadedFile != null;
        }

        public void Dispose()
        {
            _client.Logout();
        }
    }
}
