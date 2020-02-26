using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoginTest02
{
	public partial class PointDialog : Form
	{
		//private int userID;

		public PointDialog(/*DataHelper helper*/)
		{
			Debug.WriteLine("PointDialog initializing");

			InitializeComponent();

			//textBox1.Text = "" + parentModule.userID;
			//textBox1.Text = "" + this.userID;
			textBox1.Text = "" + DataHelper.userID;

			//UserSelectionFinishedEvent.Subscribe(OnUserSelectionFinishedEvent);

		}

		private void PointDialog_Load(object sender, EventArgs e)
		{

		}

		private void okButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		/*
		public void OnUserSelectionFinishedEvent(UserSelectionFinishedEventArgs e)
		{
			Debug.WriteLine("PointDialog, caught event, e.userID = " + e.userID);
			this.userID = e.userID;
			Debug.WriteLine("PointDialog, userID = " + this.userID);
		}
		*/
	}
}
