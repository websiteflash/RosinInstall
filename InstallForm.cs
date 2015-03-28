using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace RosinInstall
{
    public partial class InstallForm : Form
    {
        public InstallForm()
        {
            InitializeComponent();

            this.buttonPre.Enabled = false;
        }

        // 拷贝Rosin.dll到script目录
        // 拷贝Newtonsoft.Json.dll到script目录
        // 拷贝Rosin目录到script目录
        // 用于项目首次阶段，或者初始化
        private bool CopyAllFileToPath()
        {
            bool isInstallSuccess = false;

            string fiddlerInstallDir = this.GetFiddlerDir();
            string currentDir = System.IO.Path.GetFullPath(".\\");

            // 如果通过注册表没有获取到fiddler的安装路径，让用户去选择
            if (fiddlerInstallDir == "")
            {
                fiddlerInstallDir = this.SelectFiddlerDir();
            }

            if (fiddlerInstallDir != "")
            {
                currentDir += @"\Scripts";
                string scriptDir = fiddlerInstallDir + @"\Scripts";

                string[] fileList = { "Rosin.dll", "Newtonsoft.Json.v6.0.dll" };
                string[] dirList = { "Rosin" };

                string[] fileNoOverwriteList = { "InjectionList.xml", "InjectionRule.xml" };

                foreach (string fileName in fileList)
                {
                    string sourceFile = System.IO.Path.Combine(currentDir, fileName);
                    string destFile = System.IO.Path.Combine(scriptDir, fileName);
                    if (System.IO.File.Exists(sourceFile))
                    {
                        try
                        {
                            System.IO.File.Copy(sourceFile, destFile, true);
                        }
                        catch
                        {
                            throw new Exception("安装失败，请关闭Fiddler后再安装!");
                        }
                    }
                }

                foreach (string dirName in dirList)
                {
                    string sourceDir = System.IO.Path.Combine(currentDir, dirName);
                    string destDir = System.IO.Path.Combine(scriptDir, dirName);
                    this.CopyDirectory(sourceDir, destDir, fileNoOverwriteList);
                }

                isInstallSuccess = true;
            }

            return isInstallSuccess;
        }

        private void CopyDirectory(string srcDir, string tgtDir, string[] fileNoOverwriteList)
        {
            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("父目录不能拷贝到子目录！");
            }

            if (!source.Exists)
            {
                return;
            }

            if (!target.Exists)
            {
                target.Create();
            }

            FileInfo[] files = source.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                bool isOverWrite = true;

                foreach (string name in fileNoOverwriteList)
                {
                    if (name == files[i].Name)
                    {
                        isOverWrite = false;
                        break;
                    }
                }

                if (isOverWrite || !File.Exists(target.FullName + @"\" + files[i].Name))
                {
                    File.Copy(files[i].FullName, target.FullName + @"\" + files[i].Name, true);
                }
            }

            DirectoryInfo[] dirs = source.GetDirectories();

            for (int j = 0; j < dirs.Length; j++)
            {
                this.CopyDirectory(dirs[j].FullName, target.FullName + @"\" + dirs[j].Name, fileNoOverwriteList);
            }
        }

        private string GetFiddlerDir()
        {
            string fiddlerInstallPath = "";
            string fiddlerInstallDir = "";

            try
            {
                string softName = "Fiddler";
                string strKeyName = String.Empty;
                string softPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";

                RegistryKey regKey = Registry.LocalMachine;
                RegistryKey regSubKey = regKey.OpenSubKey(softPath + softName + ".exe", false);

                object objResult = regSubKey.GetValue(strKeyName);

                RegistryValueKind regValueKind = regSubKey.GetValueKind(strKeyName);

                if (regValueKind == Microsoft.Win32.RegistryValueKind.String)
                {
                    fiddlerInstallPath = objResult.ToString();
                }

            }
            catch
            {
                fiddlerInstallPath = "";
            }

            if (fiddlerInstallPath != "")
            {
                fiddlerInstallPath = fiddlerInstallPath.Replace("\"", "");
                fiddlerInstallDir = Path.GetDirectoryName(fiddlerInstallPath);
            }

            return fiddlerInstallDir;
        }

        private string SelectFiddlerDir()
        {
            string fiddlerInstallDir = "";

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择Fiddler的安装路径";
            folderBrowserDialog.ShowDialog();

            fiddlerInstallDir = folderBrowserDialog.SelectedPath;

            return fiddlerInstallDir;
        }

        private void buttonPre_Click(object sender, EventArgs e)
        {
            this.buttonPre.Enabled = false;
            this.buttonNext.Enabled = true;
            this.buttonCancel.Enabled = true;

            this.labelIntro.Visible = true;
            this.labelGuide.Visible = true;
            this.labelInstall.Visible = false;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            this.labelInstall.Text = "安装中，请稍后...";

            this.buttonPre.Enabled = false;
            this.buttonNext.Enabled = false;
            this.buttonCancel.Enabled = false;

            this.buttonPre.Visible = false;
            this.buttonNext.Visible = false;
            this.buttonCancel.Visible = false;

            this.labelIntro.Visible = false;
            this.labelGuide.Visible = false;
            this.labelInstall.Visible = true;

            this.buttonDone.Enabled = false;
            this.buttonDone.Visible = true;

            bool result = this.CopyAllFileToPath();

            if (result)
            {
                this.labelInstall.Text = "安装完成，感谢使用!";
                this.buttonDone.Enabled = true;
            }
            else
            {
                this.labelInstall.Text = "安装失败，请重新安装!";
                this.buttonDone.Enabled = true;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
