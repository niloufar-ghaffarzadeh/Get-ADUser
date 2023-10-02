using System;
using System.Collections.Generic;
using System.DirectoryServices;


public class UserInfo
{
    public string Username { get; set; }
    public string? EmailAddress { get; set; }
}
public class ActiveDirectoryPaginationAndSort
{
    public static IEnumerable<UserInfo> GetUsersWithFilter(int pageNumber, int pageSize, string filterProperty, string filterValue)
    {   
        try
        {
            return _GetUsersWithFilter(pageNumber, pageSize, filterProperty, filterValue);
        }
        catch (DirectoryServicesCOMException ex)
        {
            return Enumerable.Empty<UserInfo>();
        }
        catch (Exception ex)
        {
            return Enumerable.Empty<UserInfo>();
        }
    }
    private static IEnumerable<UserInfo> _GetUsersWithFilter(int pageNumber, int pageSize, string filterProperty, string filterValue)
    {
        var directoryEntry = new DirectoryEntry("LDAP://" + "Replace with your Active Directory domain Controller");
        var directorySearcher = new DirectorySearcher(directoryEntry);
        ///Step 1 : Set the LDAP filter
        directorySearcher.Filter = $"(&(objectCategory=person)(objectClass=user)({filterProperty}={filterValue}))";

        ///Step 2 : Set the starting index
        directorySearcher.PageSize = pageSize;

        ///Step 3 : Set the page size
        directorySearcher.SizeLimit = pageSize * pageNumber;

        ///Step 4 : Define the properties to retrieve
        directorySearcher.PropertiesToLoad.Add("samAccountName");
        directorySearcher.PropertiesToLoad.Add("mail");

        ///Step 5 :Set the sort result  (sort base on Username)
        directorySearcher.Sort.PropertyName = "samAccountName";

        ///Step 6 :Set the sort results in descending
        directorySearcher.Sort.Direction = SortDirection.Descending;

        ///Step 7 : Perform the search
        var searchResults = directorySearcher.FindAll();

        ///Step 8 : retrieve the user information
        for (int i = (pageNumber - 1) * pageSize; i < searchResults.Count; i++)
        {
            var result = searchResults[i];
            var userName = result.Properties["samAccountName"][0].ToString();
            var emailAddress = result.Properties["mail"][0].ToString();

            var userInfo = new UserInfo
            {
                Username = userName,
                EmailAddress = emailAddress
            };
            yield return userInfo;
        }
        searchResults.Dispose();
        directorySearcher.Dispose();
    }

}

public class Program
{
    public static void Main()
    {
        int pageSize = 10;
        int pageNumber = 1;
        string filterProperty = "department";
        string filterValue = "IT";

        try
        {
            var users = ActiveDirectoryPaginationAndSort.GetUsersWithFilter(pageNumber, pageSize, filterProperty, filterValue);
            foreach (var temp in users)
            {
                Console.WriteLine($"Username : {temp.Username},Email : {temp.EmailAddress}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occured during the search :{ex.Message}");
        }
    }
}