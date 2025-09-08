using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using RemarkableSleepScreenManager.Models;

namespace RemarkableSleepScreenManager.Services
{
    public class SshService : ISshService, IDisposable
    {
        private SshClient? _sshClient;
        private SftpClient? _sftpClient;
        private bool _disposed = false;

        public async Task<bool> TestConnectionAsync(ConnectionSettings settings)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var client = CreateSshClient(settings);
                    client.Connect();
                    var result = client.RunCommand("uname -a");
                    client.Disconnect();
                    return !string.IsNullOrEmpty(result.Result);
                });
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> ExecuteCommandAsync(ConnectionSettings settings, string command)
        {
            return await Task.Run(() =>
            {
                using var client = CreateSshClient(settings);
                client.Connect();
                var result = client.RunCommand(command);
                client.Disconnect();
                return result.Result + result.Error;
            });
        }

        public async Task UploadFileAsync(ConnectionSettings settings, string localPath, string remotePath)
        {
            await Task.Run(() =>
            {
                using var sftp = CreateSftpClient(settings);
                sftp.Connect();
                using var fs = File.OpenRead(localPath);
                sftp.UploadFile(fs, remotePath, true);
                sftp.Disconnect();
            });
        }

        public async Task UploadTextAsync(ConnectionSettings settings, string content, string remotePath)
        {
            await Task.Run(() =>
            {
                using var sftp = CreateSftpClient(settings);
                sftp.Connect();
                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
                sftp.UploadFile(ms, remotePath, true);
                sftp.Disconnect();
            });
        }

        public async Task EnsureDirectoryExistsAsync(ConnectionSettings settings, string remotePath)
        {
            await Task.Run(() =>
            {
                using var sftp = CreateSftpClient(settings);
                sftp.Connect();
                EnsureDir(sftp, remotePath);
                sftp.Disconnect();
            });
        }

        private SshClient CreateSshClient(ConnectionSettings settings)
        {
            var client = new SshClient(settings.IpAddress, settings.Username, settings.Password);
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
            return client;
        }

        private SftpClient CreateSftpClient(ConnectionSettings settings)
        {
            var client = new SftpClient(settings.IpAddress, 22, settings.Username, settings.Password);
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
            return client;
        }

        private void EnsureDir(SftpClient sftp, string path)
        {
            if (sftp.Exists(path)) return;
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string cur = "";
            foreach (var p in parts)
            {
                cur += "/" + p;
                if (!sftp.Exists(cur)) sftp.CreateDirectory(cur);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _sshClient?.Dispose();
                _sftpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
