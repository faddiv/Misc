using System.DirectoryServices;
using System.Net;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
var id = new System.DirectoryServices.Protocols.LdapDirectoryIdentifier("FADDIV", 50000);
using var c = new System.DirectoryServices.Protocols.LdapConnection(id);
c.Bind();
var response = (System.DirectoryServices.Protocols.SearchResponse)c.SendRequest(new System.DirectoryServices.Protocols.SearchRequest("CN=faddiv,DC=localtest,DC=me", "(displayName=Fad*)", System.DirectoryServices.Protocols.SearchScope.Subtree, new[] { "displayName", "employeeId" }));
foreach (System.DirectoryServices.Protocols.SearchResultEntry item in response.Entries)
{
    Console.WriteLine("{0} - {1}", item.Attributes["displayName"][0], item.Attributes["employeeId"][0]);
}
/*
var r = new DirectoryEntry("LDAP://FADDIV:50000/CN=Users,CN=faddiv,DC=localtest,DC=me");
var s = new DirectorySearcher(r, "(displayName=Fad*)", new[] { "displayName", "employeeId" }, SearchScope.Subtree);
foreach (SearchResult item in s.FindAll())
{
    Console.WriteLine("{0} - {1}", item.Properties["displayName"][0], item.Properties["employeeId"][0]);
}*/