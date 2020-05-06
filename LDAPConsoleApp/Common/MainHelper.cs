using LDAPConsoleApp.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NPinyin;

namespace LDAPConsoleApp.Common
{
    public class MainHelper
    {
        public static string domainName = ConfigurationManager.AppSettings["domainName"];
        public static string userName = ConfigurationManager.AppSettings["userName"];
        public static string userPwd = ConfigurationManager.AppSettings["userPwd"];
        public static string mainOU = ConfigurationManager.AppSettings["mainOU"];
        public static string DC1 = ConfigurationManager.AppSettings["DC1"];
        public static string DC2 = ConfigurationManager.AppSettings["DC2"];

        #region 域连接
        /// <summary>
        /// 创建AD主连接
        /// </summary>
        /// <returns></returns>
        public DirectoryEntry GetDirectoryEntry()
        {
            DirectoryEntry de = new DirectoryEntry();
            if(IsConnected(domainName, userName, userPwd, out de))
            {
                return de;
            }

            return null;
        }
        /// <summary>
        /// 是否连接到域
        /// </summary>
        /// <param name="domainName">域名或IP</param>
        /// <param name="userName">用户名</param>
        /// <param name="userPwd">密码</param>
        /// <param name="domain">域</param>
        /// <returns></returns>
        public bool IsConnected(string domainName, string userName, string userPwd, out DirectoryEntry de)
        {
            de = new DirectoryEntry();
            try
            {
                de.Path = string.Format("LDAP://{0}", domainName);
                de.Username = userName;
                de.Password = userPwd;
                de.AuthenticationType = AuthenticationTypes.Secure;

                var tmp = de.Guid;
                de.RefreshCache();

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteProgramLog("[IsConnected方法]错误信息：" + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 根据OU路径获取目录
        /// </summary>
        /// <returns></returns>
        public DirectoryEntry GetDirectoryEntryByOUPath(string ouPath)
        {
            try
            {
                string path = string.Format("LDAP://{0}/{1},DC={2},DC={3}", domainName, ouPath, DC1, DC2);
                DirectoryEntry de = new DirectoryEntry();
                if (IsExistsPath(path, userName, userPwd, out de))
                {
                    return de;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 判断指定目录是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsExistsPath(string path, string userName, string userPwd, out DirectoryEntry de)
        {
            de = new DirectoryEntry();

            try
            {
                de.Path = path;
                de.Username = userName;
                de.Password = userPwd;
                de.AuthenticationType = AuthenticationTypes.Secure;

                var tmp = de.Guid;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 根据组织单位名称获取组织单位对象
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="ou"></param>
        /// <returns></returns>
        public DirectoryEntry GetDirectoryEntryOfOU(string ou)
        {
            DirectoryEntry de = GetDirectoryEntry();
            DirectorySearcher deSearch = new DirectorySearcher(de);
            deSearch.Filter = "(&(objectClass=organizationalUnit)(OU=" + ou + "))";
            deSearch.SearchScope = SearchScope.Subtree;

            try
            {
                SearchResult result = deSearch.FindOne();
                de = new DirectoryEntry(result.Path, userName, userPwd, AuthenticationTypes.Secure);
                return de;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        ///根据组名取得用户组的对象
        ///组名
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public DirectoryEntry GetDirectoryEntryOfGroup(string groupName)
        {
            DirectoryEntry de = GetDirectoryEntry();
            DirectorySearcher deSearch = new DirectorySearcher(de);
            deSearch.Filter = "(&(objectClass=group)(cn=" + groupName + "))";
            deSearch.SearchScope = SearchScope.Subtree;

            try
            {
                SearchResult result = deSearch.FindOne();
                de = new DirectoryEntry(result.Path, userName, userPwd, AuthenticationTypes.Secure);
                return de;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        ///根据用户公共名称取得用户的对象
        ///用户公共名称
        ///如果找到该用户，则返回用户的对象；否则返回 null
        /// </summary>
        /// <param name="commonName"></param>
        /// <returns></returns>
        public DirectoryEntry GetDirectoryEntryOfUser(string commonName)
        {
            DirectoryEntry de = GetDirectoryEntry();
            DirectorySearcher deSearch = new DirectorySearcher(de);
            deSearch.Filter = "(&(&(objectCategory=person)(objectClass=user))(cn=" + commonName + "))";
            deSearch.SearchScope = SearchScope.Subtree;

            try
            {
                SearchResult result = deSearch.FindOne();
                de = new DirectoryEntry(result.Path, userName, userPwd, AuthenticationTypes.Secure);
            }
            catch
            {
                de = null;
            }
            return de;
        }
        /// <summary>
        ///根据指定目录和用户公共名称取得用户的对象
        ///用户公共名称
        ///如果找到该用户，则返回用户的对象；否则返回 null
        /// </summary>
        /// <param name="commonName"></param>
        /// <returns></returns>
        public DirectoryEntry GetDirectoryEntryOfUser(DirectoryEntry de, string commonName)
        {
            DirectorySearcher deSearch = new DirectorySearcher(de);
            deSearch.Filter = "(&(&(objectCategory=person)(objectClass=user))(cn=" + commonName + "))";
            deSearch.SearchScope = SearchScope.Subtree;

            try
            {
                SearchResult result = deSearch.FindOne();
                de = new DirectoryEntry(result.Path, userName, userPwd, AuthenticationTypes.Secure);
            }
            catch
            {
                de = null;
            }
            return de;
        }
        #endregion

        #region OU 组织单位
        /// <summary>
        /// 同步公司组织架构
        /// </summary>
        /// <param name="lstDepartment"></param>
        public void SyncOU(List<YTFullDept> lstDepartmentAll, List<YTFullDept> lstDepartment)
        {
            if(lstDepartment.IsNotNullAndGreaterThan(0))
            {
                foreach (var item in lstDepartment)
                {
                    if(!string.IsNullOrEmpty(item.FullName))
                    {
                        //不存在此组织
                        if(GetDirectoryEntryByOUPath(FullDeptNameToOUPath(item.FullName) + ",OU=" + mainOU) == null)
                        {
                            //主已经存在，所以子部门可以直接插入
                            if(item.parentId == "26")
                            {
                                DirectoryEntry mainEntry = GetDirectoryEntryByOUPath("OU=" + mainOU);
                                CreateOU(mainEntry, item.DeptName, item.DeptName);
                            }
                            else
                            {
                                YTFullDept parentModel = lstDepartmentAll.Where(it => it.id == item.parentId).FirstOrDefault();
                                if(parentModel != null)
                                {
                                    DirectoryEntry parentEntry = GetDirectoryEntryByOUPath(FullDeptNameToOUPath(parentModel.FullName) + ",OU=" + mainOU);
                                    //父部门不存在，先要创建父部门,再创建子部门组织单位
                                    if (parentEntry == null)
                                    {
                                        List<YTFullDept> lstTemp = new List<YTFullDept>();
                                        lstTemp.Add(parentModel);
                                        SyncOU(lstDepartmentAll, lstTemp);
                                    }
                                    //父部门存在,直接创建部门组织单位
                                    else
                                    {
                                        CreateOU(parentEntry, item.DeptName, item.DeptName);
                                    }
                                }
                            }
                        }
                        //存在此组织
                        else
                        {
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 创建OU
        /// </summary>
        /// <param name="parentEntry"></param>
        /// <param name="ouName"></param>
        /// <param name="description"></param>
        public void CreateOU(DirectoryEntry parentEntry, string ouName, string description)
        {
            try
            {
                DirectoryEntry ouEntry = parentEntry.Children.Add("ou=" + ouName, "organizationalUnit");
                //为创建的新OU赋值属性
                if (!String.IsNullOrEmpty(description))
                    ouEntry.Properties["description"].Value = description;
                //保存
                ouEntry.CommitChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }
        /// <summary>
        /// 修改OU名称
        /// </summary>
        /// <param name="ouName"></param>
        /// <param name="ouNewName"></param>
        public void ModifyOU(DirectoryEntry de, string ouNewName)
        {
            de.Rename("OU=" + ouNewName);
            de.CommitChanges();
            de.Close();
        }
        /// <summary>
        /// 删除OU
        /// </summary>
        /// <param name="ouName"></param>
        public void DeleteOU(DirectoryEntry de, string ouName)
        {
            try
            {
                DirectoryEntry ouEntry = de.Children.Find("OU=" + ouName);
                if (de != null)
                {
                    de.Children.Remove(ouEntry);

                    de.CommitChanges();
                }
                ouEntry.Close();
                de.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 部门主路径转换为OUPath
        /// </summary>
        /// <param name="FullDeptName"></param>
        /// <returns></returns>
        public string FullDeptNameToOUPath(string FullDeptName)
        {
            string OUPath = "";
            if(FullDeptName.IndexOf("-") > 0)
            {
                List<string> lstFullDeptName = new List<string>(FullDeptName.Split("-".ToCharArray()));
                foreach (var item in lstFullDeptName)
                {
                    OUPath = string.Format(",OU={0}", item) + OUPath;
                }

                if (OUPath.IndexOf("/") > 0)
                    return OUPath.TrimStart(",".ToCharArray()).Replace("/","-");
                else
                    return OUPath.TrimStart(",".ToCharArray());
            }
            else
            {
                if (FullDeptName.IndexOf("/") > 0)
                    return string.Format("OU={0}", FullDeptName).Replace("/","-");
                else
                    return string.Format("OU={0}", FullDeptName);
            }
        }
        #endregion

        #region User 用户
        public void SyncUser(List<YTFullDept> lstDepartmentAll, List<YTStaff> lstUser)
        {
            if(lstUser.IsNotNullAndGreaterThan(0))
            {
                foreach (var item in lstUser)
                {
                    YTFullDept DeptModel = lstDepartmentAll.Where(it => it.id == item.StaffDepartMent).FirstOrDefault();
                    if(DeptModel != null)
                    {
                        //判断用户部门是否在AD域的组织架构里面
                        DirectoryEntry deptEntry = GetDirectoryEntryByOUPath(FullDeptNameToOUPath(DeptModel.FullName) + ",OU=" + mainOU);
                        //部门组织存在则进行用户操作，部门组织不存在则不进行操作
                        if (deptEntry != null)
                        {
                            //判断用户是否存在
                            DirectoryEntry userEntry = GetDirectoryEntryOfUser(deptEntry, item.StaffName);
                            //用户不存在则执行插入
                            if(userEntry == null)
                            {
                                CreateNewUser(deptEntry, item, DeptModel.DeptName, "");
                            }
                            //用户存在则进行更新
                            else
                            {

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个新用户
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="name"></param>
        /// <param name="login"></param>
        /// <param name="email"></param>
        /// <param name="group"></param>
        public void CreateNewUser(DirectoryEntry parentEntry, YTStaff staffModel, string DeptName, string group)
        {
            /*
             LDAP Property Name                 Description                         Data Type
                givenName                           First Name                          String
                initials                            Initials                            String
                sn                                  Last name                           String
                displayName                         Display name                        String
                description                         Description                         String
                physicalDeliveryOfficeName          Office                              String
                telephoneNumber                     Telephone number                    String
                otherTelephone                      Other Telephone numbers             String
                mail                                E-mail                              String
                wWWHomePage                         Web page                            String
                url                                 Other Web pages                     String
                streetAddress                       Street                              String
                postOfficeBox                       P.O. Box                            String
                l                                   City                                String
                st                                  State/province                      String
                postalCode                          Zip/Postal Code                     String
                c, co, countryCode                  Country/region                      String
                userPrincipalName                   User logon name                     String
                sAMAccountName                      pre-Windows 2000 logon name         String
                userAccountControl                  Account disabled?                   Boolean
                profilePath                         User Profile path                   String
                scriptPath                          Logon script                        String
                homeDirectory                       Home folder, local path             String
                homeDrive                           Home folder, Connect, Drive         String
                homeDirectory                       Home folder, Connect, To:           String
                title                               Title                               String
                department                          Department                          String
                company                             Company                             String
                manager                             Manager                             String
                mobile                              Mobile                              String
                facsimileTelephoneNumber            Fax                                 String
                info                                Notes                               String
             */
            string pinyin = Pinyin.GetPinyin(staffModel.StaffName).Replace(" ", "");
            /// 1. Create user account
            DirectoryEntry newuser = parentEntry.Children.Add("CN=" + staffModel.StaffName, "user");

            /// 2. Set properties
            SetProperty(newuser, "title", staffModel.PartName);
            if(!string.IsNullOrEmpty(staffModel.StaffTel))
                SetProperty(newuser, "telephoneNumber", staffModel.StaffTel);
            SetProperty(newuser, "givenName", staffModel.StaffName);
            SetProperty(newuser, "displayName", staffModel.StaffName);
            SetProperty(newuser, "department", DeptName);
            SetProperty(newuser, "name", staffModel.StaffName);
            SetProperty(newuser, "sAMAccountName", pinyin);
            SetProperty(newuser, "employeeID", staffModel.staffNum);
            SetProperty(newuser, "userPrincipalName", pinyin + "@test.com");
            SetProperty(newuser, "mobile", staffModel.StaffPhone);
            newuser.CommitChanges();

            /// 3. Enable account           
            EnableAccount(newuser);

            /// 4. Set password
            SetPassword(newuser, "123Qweasd");

            /// 5. Add user account to groups
            if(!string.IsNullOrEmpty(group))
                AddUserToGroup(parentEntry, newuser, group);

            newuser.Close();
            parentEntry.Close();
        }
        /// <summary>
        /// 设置用户新密码
        /// </summary>
        /// <param name="de"></param>
        /// <param name="password"></param>
        public void SetPassword(DirectoryEntry de, string password)
        {
            try
            {
                object ret = de.Invoke("SetPassword", new object[] { password });
                
                de.CommitChanges();
                de.Close();
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }
        /// <summary>
        /// 启用用户帐号
        /// </summary>
        /// <param name="de"></param>
        public void EnableAccount(DirectoryEntry de)
        {
            //UF_DONT_EXPIRE_PASSWD 0x10000
            int exp = (int)de.Properties["userAccountControl"].Value;
            de.Properties["userAccountControl"].Value = exp | 0x0001;
            de.CommitChanges();
            //UF_ACCOUNTDISABLE 0x0002
            int val = (int)de.Properties["userAccountControl"].Value;
            de.Properties["userAccountControl"].Value = val & ~0x0002;
            de.CommitChanges();
        }
        /// <summary>
        /// 禁用一个帐号
        /// </summary>
        /// <param name="EmployeeID"></param>
        public void DisableAccount(string EmployeeID)
        {
            DirectoryEntry de = GetDirectoryEntry();
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = "(&(objectCategory=Person)(objectClass=user)(employeeID=" + EmployeeID + "))";
            ds.SearchScope = SearchScope.Subtree;
            SearchResult results = ds.FindOne();

            if (results != null)
            {
                DirectoryEntry dey = new DirectoryEntry(results.Path, userName, userPwd, AuthenticationTypes.Secure);
                int val = (int)dey.Properties["userAccountControl"].Value;
                dey.Properties["userAccountControl"].Value = val | 0x0002;
                dey.Properties["msExchHideFromAddressLists"].Value = "TRUE";
                dey.CommitChanges();
                dey.Close();
            }

            de.Close();
        }
        /// <summary>
        /// 添加用户到组
        /// </summary>
        /// <param name="de"></param>
        /// <param name="deUser"></param>
        /// <param name="GroupName"></param>
        public void AddUserToGroup(DirectoryEntry de, DirectoryEntry deUser, string GroupName)
        {
            DirectorySearcher deSearch = new DirectorySearcher();
            deSearch.SearchRoot = de;
            deSearch.Filter = "(&(objectClass=group) (cn=" + GroupName + "))";
            SearchResultCollection results = deSearch.FindAll();

            bool isGroupMember = false;

            if (results.Count > 0)
            {
                DirectoryEntry group = new DirectoryEntry(results[0].Path, userName, userPwd, AuthenticationTypes.Secure);

                object members = group.Invoke("Members", null);
                foreach (object member in (IEnumerable)members)
                {
                    DirectoryEntry x = new DirectoryEntry(member);
                    if (x.Name != deUser.Name)
                    {
                        isGroupMember = false;
                    }
                    else
                    {
                        isGroupMember = true;
                        break;
                    }
                }

                if (!isGroupMember)
                {
                    group.Invoke("Add", new object[] { deUser.Path.ToString() });
                }
                group.Close();
            }
            return;
        }
        #endregion

        #region 修改用户
        /// <summary>
        /// 修改用户属性
        /// </summary>
        /// <param name="de"></param>
        /// <param name="PropertyName"></param>
        /// <param name="PropertyValue"></param>
        public void SetProperty(DirectoryEntry de, string PropertyName, string PropertyValue)
        {
            if (PropertyValue != null)
            {
                if (de.Properties.Contains(PropertyName))
                {
                    de.Properties[PropertyName][0] = PropertyValue;
                }
                else
                {
                    de.Properties[PropertyName].Add(PropertyValue);
                }
            }
        }
        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="department"></param>
        /// <param name="title"></param>
        /// <param name="company"></param>
        public void ModifyUser(string employeeID, string department, string title, string company)
        {
            DirectoryEntry de = GetDirectoryEntry();
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = "(&(objectCategory=Person)(objectClass=user)(employeeID=" + employeeID + "))";
            ds.SearchScope = SearchScope.Subtree;
            SearchResult results = ds.FindOne();

            if (results != null)
            {
                DirectoryEntry dey = new DirectoryEntry(results.Path, userName, userPwd, AuthenticationTypes.Secure);
                SetProperty(dey, "department", department);
                SetProperty(dey, "title", title);
                SetProperty(dey, "company", company);
                dey.CommitChanges();
                dey.Close();
            }

            de.Close();
        }
        /// <summary>
        /// 检验Email格式是否正确
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        public bool IsEmail(string mail)
        {
            Regex mailPattern = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            return mailPattern.IsMatch(mail);
        }
        /// <summary>
        /// 搜索被修改过的用户
        /// </summary>
        /// <param name="fromdate"></param>
        /// <returns></returns>
        public DataTable GetModifiedUsers(DateTime fromdate)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("EmployeeID");
            dt.Columns.Add("Name");
            dt.Columns.Add("Email");

            DirectoryEntry de = GetDirectoryEntry();
            DirectorySearcher ds = new DirectorySearcher(de);

            StringBuilder filter = new StringBuilder();
            filter.Append("(&(objectCategory=Person)(objectClass=user)(whenChanged>=");
            filter.Append(ToADDateString(fromdate));
            filter.Append("))");

            ds.Filter = filter.ToString();
            ds.SearchScope = SearchScope.Subtree;
            SearchResultCollection results = ds.FindAll();

            foreach (SearchResult result in results)
            {
                DataRow dr = dt.NewRow();
                DirectoryEntry dey = new DirectoryEntry(result.Path, userName, userPwd, AuthenticationTypes.Secure);
                dr["EmployeeID"] = dey.Properties["employeeID"].Value;
                dr["Name"] = dey.Properties["givenname"].Value;
                dr["Email"] = dey.Properties["mail"].Value;
                dt.Rows.Add(dr);
                dey.Close();
            }

            de.Close();
            return dt;
        }
        /// <summary>
        /// 格式化AD的时间
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string ToADDateString(DateTime date)
        {
            string year = date.Year.ToString();
            int month = date.Month;
            int day = date.Day;

            StringBuilder sb = new StringBuilder();
            sb.Append(year);
            if (month < 10)
            {
                sb.Append("0");
            }
            sb.Append(month.ToString());
            if (day < 10)
            {
                sb.Append("0");
            }
            sb.Append(day.ToString());
            sb.Append("000000.0Z");
            return sb.ToString();
        }
        #endregion

        #region 修改组
        /// <summary>
        /// 获取AD组
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="organizeUnit"></param>
        /// <returns></returns>
        public DirectoryEntry GetADGroupInOU(string groupName, string organizeUnit)
        {
            if (!String.IsNullOrEmpty(groupName))
            {
                DirectoryEntry de = GetDirectoryEntry();
                DirectorySearcher deSearch = new DirectorySearcher(de);
                deSearch.Filter = "(&(objectClass=group)(cn=" + groupName.Replace("\\", "") + "))";
                deSearch.SearchScope = SearchScope.Subtree;
                try
                {
                    SearchResult result = deSearch.FindOne();
                    if (result != null)
                    {
                        de = new DirectoryEntry(result.Path, userName, userPwd);
                    }
                    else
                    {
                        return null;
                    }
                    return de;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion
    }

    /// <summary>
    /// 类型
    /// </summary>
    public enum TypeEnum : int
    {
        /// <summary>
        /// 组织单位
        /// </summary>
        OU = 1,

        /// <summary>
        /// 用户
        /// </summary>
        USER = 2,

        /// <summary>
        /// 计算机
        /// </summary>
        COMPUTER = 3
    }
    /// <summary>
    /// Ad域信息实体
    /// </summary>
    public class AdModel
    {
        public AdModel(string id, string name, int typeId, string parentId)
        {
            Id = id;
            Name = name;
            TypeId = typeId;
            ParentId = parentId;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public int TypeId { get; set; }

        public string ParentId { get; set; }
    }
}
