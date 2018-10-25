using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Net.Mail;
using UserMan;
using PrintingService.Interface;
using System.Collections;
using System.Runtime.Remoting.Channels;
using System.Threading;
using NLog;
using System.Xml.Serialization;
using ZFPCOMLib;
using Firebird;
using FirebirdSql.Data.FirebirdClient;

namespace UserManService
{
    public partial class MikrotikService : ServiceBase
    {
        public string profileName;
        public Random r = new Random(Environment.TickCount);
        public FileSystemWatcher watcher;
        public StreamWriter sw;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int maxFiscalNumber = 0;
        public System.Timers.Timer FiscalTimer = new System.Timers.Timer(5000);

        public List<User> ActivUsers = new List<User>();
        FP fp = new FP();

        int curentFiscalNumber = 0;

        public MikrotikService()
        {
            InitializeComponent();
        }
       
        protected override void OnStart(string[] args)
        {

            if (Properties.Settings.Default.GarsonEnable)
            {
                logger.Info("Start");
                FiscalTimer.Enabled = true;
                FiscalTimer.Elapsed += new System.Timers.ElapsedEventHandler(FiscalTimer_Tick);
                FiscalTimer.Start();
                logger.Info(FiscalTimer.Enabled.ToString());
            }
            if (Properties.Settings.Default.PSEnable)
            {
                useFolder();
            }


        }



        void useFolder()
        {
            // Provjeravam da li postoji folder, ako ne napravim ga da servis ne puca
            logger.Info("Servis startovan");
            logger.Info(" ");

            logger.Info("Povjeravam postojanje foldera");
            if (!Directory.Exists(Properties.Settings.Default.GarsonFolder))
            {
                logger.Info("Pravim folder " + Properties.Settings.Default.GarsonFolder);
                Directory.CreateDirectory(Properties.Settings.Default.GarsonFolder);
                logger.Info("Folder uspjesno kreiran");
            }

            logger.Info("Protect file");
            // Zastita da folder ne bude obrisan
            sw = new StreamWriter(Properties.Settings.Default.GarsonFolder + "\\protect.temp");

            // Kada se doda novi txt u folder da pokrenem metod za dodavanje usera
            watcher = new FileSystemWatcher();

            watcher.Path = Properties.Settings.Default.GarsonFolder;

            watcher.Filter = "*.txt";

            watcher.Created += new FileSystemEventHandler(watcher_Created);
            watcher.EnableRaisingEvents = true;
        }
        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            logger.Info("Sleep 500");
            Thread.Sleep(500);
            // Procitaj file iz foldera
            try
            {
                logger.Info("Procitaj file " + e.FullPath);
                profileName = File.ReadAllText(e.FullPath);
                logger.Info("File procitan");

                logger.Info("Obrisi file " + e.FullPath);
                File.Delete(e.FullPath);
                logger.Info("File obrisan");

                
                profileName = profileName.Substring(0, profileName.Length - 2);
                logger.Info("Procitani profil " + profileName);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                //sendErrorToMail(ex);
                return;
            }

            MK mk;
            // Logujem se na MK, procitaj profile, provjeri da li postoji profil i dodaj novog korisnika
            try
            {
                logger.Info("Povezivanje na mikrotik " + Properties.Settings.Default.RouterIp);
                mk = new MK(Properties.Settings.Default.RouterIp);
                logger.Info("Uspjesno povezan na mikrotik");
                logger.Info("Logovanje na mikrotik ");
                bool login = mk.Login(Properties.Settings.Default.ApiUser, Properties.Settings.Default.ApiPassword);
                logger.Info("Login = " + login.ToString());

                // Citam profile
                List<Profile> profiles = null;
                
                if (login)
                {
                    logger.Info("Procitaj profile sa mikrotika");
                    profiles = Profile.GetProfileMT(mk);
                    foreach (var item in profiles)
                    {
                        logger.Info(item.Name);
                    }
                    logger.Info("Profili procitani ");
                }
                else
                {
                    logger.Error("Neuspjesno logovanje na mikrotik");
                    return;
                }

                // Provjeri da li postoji trazeni profil i napravi usera
                Profile p = profiles.Find(x => x.Name == profileName);
                User u = null;
                if (p!=null)
                {
                    logger.Info("Dodavanje korisnika ");
                    u = AddUser(6, 3, p, mk);
                    logger.Info("Korisnik uspjesno dodat");
                }
                else
                {
                    logger.Error("U profilima ne postoji " + profileName);
                    return;
                }

                // ako je user uspjesto kreiran odstampaj ga
                if (u!=null)
                {
                    logger.Info("Stampanje slipa");
                    print(u.Username, u.Password, p.Name);
                    logger.Info("Slip uspjesno odstampan. Username = " + u.Username + " , Password = " + u.Password + " Profil = " + p.Name);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return;
            }
        }

        protected override void OnStop()
        {
            if (Properties.Settings.Default.GarsonEnable)
            {
                FiscalTimer.Enabled = false;
            }
            else
            {
                sw.Close();
                logger.Info("Protect file - off");
                File.Delete(Properties.Settings.Default.GarsonFolder + "\\protect.temp");
                logger.Info("Servis stopiran");
            } 
        }

        public string GeneratePassword(int len)
        {

            string s = "0123456789";
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= len; i++)
            {
                int idx = r.Next(0, 9);
                sb.Append(s.Substring(idx, 1));
            }
            return sb.ToString();
        }
        public User AddUser(int lenUserName, int lenUserPassword, Profile ActivProfile, MK mikrotik)
        {
            string username = GeneratePassword(lenUserName);
            string password = "password";
            User u = null;
            try
            {
                
                mikrotik.Send("/ip/hotspot/user/add");
                mikrotik.Send("=profile=" + ActivProfile.Name);
                mikrotik.Send("=name=" + username);
                mikrotik.Send("=limit-uptime=" + ActivProfile.UpTime);
                mikrotik.Send("=password=" + password, true);

                List<string> res = mikrotik.Read();
                foreach (var item in res)
                {
                    logger.Info(item.ToString());
                }

                u = new User();
                u.Username = username;
                u.Password = password;
            }
            catch (Exception ex)
            {
                
                return u;
            }
            return u;
        }

        // koristi se za fiskalni 
        public User AddUser(string username, string password, Profile ActivProfile, MK mikrotik)
        {

            User u = null;
            try
            {

                mikrotik.Send("/ip/hotspot/user/add");
                mikrotik.Send("=profile=" + ActivProfile.Name);
                mikrotik.Send("=name=" + username);
                mikrotik.Send("=limit-uptime=" + ActivProfile.UpTime);
                mikrotik.Send("=password=" + password, true);

                List<string> res = mikrotik.Read();

                foreach (var item in res)
                {
                    logger.Info(item.ToString());
                }

                u = new User();
                u.Username = username;
                u.Password = password;
            }
            catch (Exception ex)
            {

                return u;
            }
            return u;
        }

        public void print(string username, string password, string profileName)
        {
            var ps = (IPrintingService)Activator.GetObject(typeof(IPrintingService), "tcp://" + Properties.Settings.Default.PSHost + ":5555/PrintingService");
            IDictionary props = ChannelServices.GetChannelSinkProperties(ps);

            props["timeout"] = 5000;
            props["connectionTimeout"] = 3000;

            var txtDoc = new TextDocument();
            txtDoc.Description = "WiFi";
            txtDoc.User = "Kraft NT";

            // koristim za boldovani text kod komandi
            txtDoc.LineFormats.Add("{\\Ac}"); // index 0
            txtDoc.LineFormats.Add("{\\sbhw\\Ac}"); // index 1

            string[] str =Properties.Settings.Default.Header.Split('\\');

            // stampam zaglavlje
            foreach (string line in str)
            {
                txtDoc.Commands.Add(new PrintLineCommand(0, new string[] { line }));
            }

            txtDoc.Commands.Add(new PrintLineCommand(0, new string[] { "================================" }));

            txtDoc.Commands.Add(new PrintLineCommand(1, new string[] { "Password: " + username }));
            //txtDoc.Commands.Add(new PrintLineCommand(1, new string[] { "Password: " + password }));

            txtDoc.Commands.Add(new PrintLineCommand(0, new string[] { "================================" }));
            txtDoc.Commands.Add(new PrintLineCommand(0, new string[] { profileName + "" }));

            //txtDoc.LineFormats.Add("{\\sbh\\Ac}");
            //txtDoc.LineFormats.Add("{\\Ac}");
            //foreach (string s in str)
            //{
            //    txtDoc.Commands.Add(new PrintLineCommand(0,new string[] {s}));
            //}

            //txtDoc.Commands.Add(new PrintLineCommand(0, new string[] { "Test 1" }));
            txtDoc.Commands.Add(new LineFeedCommand(3));
            txtDoc.Commands.Add(new CutCommand(true));
            ps.QueueTextDocument(Properties.Settings.Default.PSName, txtDoc);
        }

        private int GetGarsonLastFiscalNumber()
        {
            int fiscalNumber=0;

            DbConnSettings dbc = new DbConnSettings
            {
                DatabaseHost = Properties.Settings.Default.GarsonHost,
                DatabasePath = Properties.Settings.Default.GarsonDatabase,
                Username = "sysdba",
                Password = "masterkey"
            };

            DbConn conn = new DbConn(dbc);

            if (conn.TestConnection())
            {
                FbCommand cmd = new FbCommand();
                cmd.CommandText = @"select max(broj_isecka) from r_racuni_dnevni where id_radne_stanice="+Properties.Settings.Default.Stanica1.ToString();
                FbCommand cmd2 = new FbCommand();
                cmd2.CommandText = @"select max(broj_isecka) from r_racuni_dnevni where id_radne_stanice=" + Properties.Settings.Default.Stanica2.ToString();

                object o = conn.ExeScalar(cmd);
                if (o==DBNull.Value)
                {
                    o = 0;
                }
                object o2 = conn.ExeScalar(cmd2);
                if (o2 == DBNull.Value)
                {
                    o2 = 0;
                }
                logger.Info("MAX broj fiskalnog isjecka = " + ((int)o + (int)o2).ToString());
                if (!(o==DBNull.Value))
                {
                    fiscalNumber = (int)o+(int)o2 + Properties.Settings.Default.GarsonFiscalNumber;
                    
                }
            }

            return fiscalNumber;
        }

        private void FiscalTimer_Tick(object sender, EventArgs e)
        {
            logger.Info("START: FiscalTimer_Tick");

            try
            {              
                int fiskalNumberFromDatabes = GetGarsonLastFiscalNumber();

                if (fiskalNumberFromDatabes>curentFiscalNumber)
                {
                    curentFiscalNumber = fiskalNumberFromDatabes;

                    string username =curentFiscalNumber.ToString("000000");
                    logger.Info("New user = " + username);
                    MK mt = new MK(Properties.Settings.Default.RouterIp, Properties.Settings.Default.ApiPort);
                    bool ok = mt.Login(Properties.Settings.Default.ApiUser, Properties.Settings.Default.ApiPassword);
                    logger.Info("Login mt = " + ok.ToString());
                    if (ok)
                    {
                        List<Profile> profiles = Profile.GetProfileMT(mt);
                        Profile p = profiles.Where(x => x.Name == Properties.Settings.Default.GarsonProfile).FirstOrDefault();
                        AddUser(username, "password", p, mt);
                        logger.Info("addUser = (" + username + ",password," + p.Name + ")");
                        string[] stri = p.UpTime.Split(':');
                        int durationInHours;
                        int.TryParse(stri[0], out durationInHours);
                        ActivUsers.Add(new User() { Username = username, Created = DateTime.Now, DurationInHours = durationInHours });
                    }

                }               

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private void RemoveUser(User item, MK mt)
        {
            try
            {
                mt.Send("/ip/hotspot/user/remove");
                mt.Send("=numbers=" + item.Username);
            }
            catch (Exception)
            {
                
            }
        }

        public List<Profile> GetAllProfiles()
        {
            MK mt = new MK(Properties.Settings.Default.RouterIp, Properties.Settings.Default.ApiPort);
            if (mt.Login(Properties.Settings.Default.ApiUser, Properties.Settings.Default.ApiPassword))
            {
                return Profile.GetProfileMT(mt);
            }
            else
            {
                return null;
            }
        }

    }
}
