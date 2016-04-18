using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace AuthenticateUsers
{
    public class AuthenticateUser
    {
        // used to set up AD context
        private string _host_domainIP = string.Empty; // the AD domain ip address with port number
        private string _domain_user = string.Empty;   // user name giving rights to search AD
        private string _domain_pw = string.Empty;     // password for AD account giving rights to search AD

        private string _userName = string.Empty;
        private string _password = string.Empty;
        private string _first = string.Empty;
        private string _last = string.Empty;

        private string _errMessage = string.Empty;

        private ContextType _context;
        private PrincipalContext _myContext = null;

        private UserPrincipal _up;

        /// <summary>
        /// Instantiate AuthenticateUser.
        /// </summary>
        public AuthenticateUser(string domain, string domain_user, string domain_pw, string userName, string passWord)
        {
            _userName = userName;
            _password = passWord;
            _domain_user = domain_user;
            _domain_pw = domain_pw;

            //determine domain and set _host_domainIP. ContextType = domain
            _host_domainIP = domain.ToLower();
            _context = ContextType.Domain;

            if (_host_domainIP == string.Empty)
                this._errMessage = "Incorrect domain specified.";
            else
            {
                //set the context
                _myContext = this.SetContext(_context, _host_domainIP);
                _up = this.SetUP(_myContext);
            }
        }


        /// <summary>
        /// Instantiate AuthenticateUser OverRide.
        /// </summary>
        public AuthenticateUser(string domain, string domain_user, string domain_pw)
        {
            _domain_user = domain_user;
            _domain_pw = domain_pw;

            //determine domain and set _host_domainIP. ContextType = domain
            _host_domainIP = domain.ToLower();
            _context = ContextType.Domain;

            if (_host_domainIP == string.Empty)
                this._errMessage = "Incorrect domain specified.";
            else
            {
                //set the context
                _myContext = this.SetContext(_context, _host_domainIP);
                //_up = this.SetUP(_myContext);
            }
        }


        /// <summary>
        /// Validate credentials passed.
        /// </summary>
        public bool ValidateCredentials()
        {
            if (_myContext != null)
            {
                if (_myContext.ValidateCredentials(_userName, _password) == false)
                {
                    this._errMessage = "Invalid username or password.";
                    return false;
                }
                else
                    return true;
            }
            else
            {
                return false;
            }

        }


        PrincipalContext SetContext(ContextType sContext, string sServer)
        {
            PrincipalContext oContext;

            //must pass a username and password that has access to all the domains
            oContext = new PrincipalContext(sContext, sServer, _domain_user, _domain_pw);

            return oContext;

        }

        UserPrincipal SetUP(PrincipalContext oContext)
        {
            // Find the principal object for which you wish to check membership!
            UserPrincipal up = UserPrincipal.FindByIdentity(oContext, IdentityType.SamAccountName, _userName);

            return up;
        }

        public string GetErrorMsg()
        {
            return this._errMessage;
        }

        /// <summary>
        /// Gets the value of an Active Directory item.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="key">The name of the active directory value to retrieve.</param>
        /// <returns></returns>
        ///         
        public string GetValue(string userName, string key)
        {
            string sValue = string.Empty;

            using (DirectoryEntry de = new DirectoryEntry("LDAP://" + this._host_domainIP, this._domain_user, this._domain_pw))
            {
                using (DirectorySearcher adSearch = new DirectorySearcher(de))
                {
                    adSearch.Filter = "(sAMAccountName=" + userName + ")";
                    SearchResult adSearchResult = adSearch.FindOne();

                    if (adSearchResult != null)
                    {
                        using (DirectoryEntry de1 = adSearchResult.GetDirectoryEntry())
                        {
                            sValue = Convert.ToString(de1.Properties[key].Value);
                        }
                    }
                    else
                    {
                        //invalid object
                        sValue = "";
                    }
                }
            }

            return sValue;
        }


        /// <summary>
        /// Validates group membership.
        /// </summary>
        /// <param name="groupName">The name of the active directory group.</param>
        /// <returns></returns>
        public Boolean IsGroupMember(string groupName)
        {
            bool isMember = false;
            string _groupName = string.Empty;

            // Retrieve the Group principal object for the group you need.
            // This will verify that the group exists on the specific domain.  If gp = null, then it did not find that group.
            _groupName = groupName.ToString();
            GroupPrincipal gp = GroupPrincipal.FindByIdentity(_myContext, _groupName);
            if (gp != null)
                isMember = gp.Members.Contains(_up);

            if (gp != null)
                gp.Dispose();

            return isMember;
        }


        /// <summary>
        /// Returns a list of Users for a specified group.
        /// </summary>
        /// <param name="groupName">The name of the active directory group.</param>
        /// <returns></returns>
        public List<string> ListUsersForGroup(string groupName)
        {
            string _groupName = string.Empty;
            List<string> _members = new List<string>();

            // Retrieve the Group principal object for the group you need.
            // This will verify that the group exists on the specific domain.  If gp = null, then it did not find that group.
            _groupName = groupName.ToString();
            GroupPrincipal gp = GroupPrincipal.FindByIdentity(_myContext, _groupName);
            if (gp != null)
            {
                foreach (Principal p in gp.GetMembers(true))
                {
                    _members.Add(p.SamAccountName.ToString());
                }
            }
            
            if (gp != null)
                gp.Dispose();

            return _members;
        }

        /// <summary>
        /// Returns true or false for active user.
        /// </summary>
        /// <param name="sUserName">The user name to test.</param>
        /// <returns></returns>
        public bool IsUserActive(string sUserName)
        {
            bool disabled = false;

            using (DirectoryEntry de = new DirectoryEntry("LDAP://" + this._host_domainIP, this._domain_user, this._domain_pw))
            {
                using (DirectorySearcher adSearch = new DirectorySearcher(de))
                {
                    adSearch.Filter = "(sAMAccountName=" + sUserName + ")";
                    adSearch.PropertiesToLoad.Add("userAccountControl");
                    SearchResult adSearchResult = adSearch.FindOne();

                    int userAccountControl = Convert.ToInt32(adSearchResult.Properties["userAccountControl"][0]);
                    disabled = ((userAccountControl & 2) > 0);
                }
            }

            if (disabled)
                return false; //not active
            else
                return true; //active
        }

        /// <summary>
        /// Returns value of samAccountName.
        /// </summary>
        /// <param name="EmployeeNumber">EmployeeNumber to search on.</param>
        public string GetUserName(string EmployeeNumber)
        {
            string _userName = string.Empty;
            using (DirectoryEntry de = new DirectoryEntry("LDAP://" + this._host_domainIP, this._domain_user, this._domain_pw))
            {
                using (DirectorySearcher adSearch = new DirectorySearcher(de))
                {
                    adSearch.Filter = "(employeeNumber=" + EmployeeNumber + ")";
                    SearchResult adSearchResult = adSearch.FindOne();

                    _userName = adSearchResult.Properties["sAMAccountName"][0].ToString();
                }
            }

            return _userName;
        }
    }
}