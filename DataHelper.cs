using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginTest02
{
	public class DataHelper
	{
		public static int userID;
		public static String userName;

		/*
		public delegate void UserLogin();
		public static event UserLogin OnUserLogin;

		public static void raiseLogin()
		{
			if (OnUserLogin != null)
			{ 
				OnUserLogin(); 
			}
		}
		*/

		public delegate void UserLoginDelegate();
		public static event UserLoginDelegate UserLoginHandler;

		public static void UserLogin(/*int uID, String uName*/)
		{
			//userID = uID;
			//userName = uName;
			UserLoginHandler?.Invoke();
		}
	}
}
