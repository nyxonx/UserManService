using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UserManService;
using UserMan;

namespace HotSpotUserApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MikrotikService mtService = new MikrotikService();
            List<Profile> p = mtService.GetAllProfiles();

            ImageList iList = new ImageList();
            iList.ImageSize = new Size(64, 64);
            iList.ColorDepth = ColorDepth.Depth32Bit;

            lvProfile.LargeImageList = iList;

            foreach (Profile item in p)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.BackColor = Color.Green;
                lvi.Text = item.Name;
                lvProfile.Items.Add(lvi);
            }
        }
    }
}
