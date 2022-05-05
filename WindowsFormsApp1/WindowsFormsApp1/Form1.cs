using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal; 

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public List<string> lastpath = new List<string>();
        public bool a = true;
        public string moveTo = "";

        public Form1()
        {
            InitializeComponent();
            try
            {
                FolderBrowserDialog FBD = new FolderBrowserDialog();
                if (FBD.ShowDialog() == DialogResult.OK)
                {
                    string path = FBD.SelectedPath;
                    PopulateTreeView(path);
                }
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message, "Ошибка");
            }
        }
        
        public void Paths(string paths)
        {
            lastpath.Add(paths);
        }
        private void PopulateTreeView(string path)
        {
            TreeNode rootNode;
            DirectoryInfo info = new DirectoryInfo(path);
            
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                GetDirectories(info.GetDirectories(), rootNode);
                treeView1.Nodes.Add(rootNode);
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs,
            TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            listView1.Items.Clear();
            fill(nodeDirInfo);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        void fill(DirectoryInfo nodeDirInfo)
        {
            ListViewItem item = null;
            ListViewItem.ListViewSubItem[] subItems;
            
            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                    {new ListViewItem.ListViewSubItem(item, GetFolderOwner(dir)),
                     new ListViewItem.ListViewSubItem(item, dir.LastWriteTime.ToShortDateString()),
                     new ListViewItem.ListViewSubItem(item, DirSize(dir).ToString() + " байт")};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);

                subItems = new ListViewItem.ListViewSubItem[]
                    { new ListViewItem.ListViewSubItem(item, GetFileOwner(file)),
                 new ListViewItem.ListViewSubItem(item, file.LastWriteTime.ToShortDateString()),
                 new ListViewItem.ListViewSubItem(item, file.Length.ToString() + " байт"),};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            if (a)
            {
                Paths(nodeDirInfo.FullName);
            }
            textBox1.Text = nodeDirInfo.FullName;
        }
        static string GetFolderOwner(DirectoryInfo folder)
        {
            var ds = folder.GetAccessControl(AccessControlSections.Owner);
            var nta = ds.GetOwner(typeof(NTAccount));
            string folder_owner = nta.ToString();
            folder_owner = folder_owner.Split('\\')[1];
            return folder_owner;
        }

        static string GetFileOwner(FileInfo fileinfo)
        {
            FileSecurity fileSecurity = fileinfo.GetAccessControl();
            IdentityReference identityReference = fileSecurity.GetOwner(typeof(NTAccount));
            string owner = identityReference.Value;
            owner = owner.Split('\\')[1];
            return owner;
        }

        static long DirSize(DirectoryInfo dir)
        {
            return dir.GetFiles().Sum(fi => fi.Length) +
                   dir.GetDirectories().Sum(di => DirSize(di));
        }

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(textBox1.Text);
                
                if (textBox1.Text != "" && dir.Exists)
                {
                    PopulateTreeView(textBox1.Text);
                    listView1.Items.Clear();
                    treeView1.Nodes[0].Remove();
                    fill(dir);
                }
                else
                {
                    MessageBox.Show("Пути не существует", "Ошибка");
                }
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message, "Ошибка");
            }
        }

        private void button2_MouseClick(object sender, MouseEventArgs e)
        {
            a = false;
            if(lastpath.Count != 0)
            {
                DirectoryInfo dir = new DirectoryInfo(lastpath[lastpath.Count - 1]);
                lastpath.RemoveAt(lastpath.Count - 1);
                PopulateTreeView(dir.FullName);
                listView1.Items.Clear();
                treeView1.Nodes[0].Remove();
                fill(dir);
                a = true;
            }
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = listView1.SelectedItems[0].Text;
            string del_file = lastpath[lastpath.Count-1] + @"\" + name;
            FileInfo fInfo = new FileInfo(del_file);
            fInfo.Delete();
            listView1.Items.Remove(listView1.SelectedItems[0]);
        }

        private void этоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = listView1.SelectedItems[0].Text;
            moveTo = lastpath[lastpath.Count - 1] + @"\" + name;
        }

        private void кудаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string move = listView1.SelectedItems[0].Text;
            DirectoryInfo dir = new DirectoryInfo(lastpath[lastpath.Count - 1] + @"\" + move + @"\" + moveTo.Substring(moveTo.LastIndexOf('\\')));
            new FileInfo(moveTo).MoveTo(dir.FullName);
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(MousePosition);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string search_file = textBox2.Text;
                string[] allFoundFiles = Directory.GetFiles(lastpath[lastpath.Count - 1], search_file, SearchOption.AllDirectories);
                listView1.Items.Clear();
                foreach (string file in allFoundFiles)
                {
                    PopulateTreeView(file.Remove(file.LastIndexOf('\\')));
                    fill(new DirectoryInfo(file.Remove(file.LastIndexOf('\\'))));

                }
                ListView.ListViewItemCollection toFindColection = listView1.Items;
                for (int i = 0; i < toFindColection.Count; i++)
                {
                    if (toFindColection[i].Text == search_file)
                    {
                        listView1.Select();
                        toFindColection[i].Selected = true;
                        break;
                    }
                }
                treeView1.Nodes[0].Remove();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Ошибка");
            }
        }
    }
}

