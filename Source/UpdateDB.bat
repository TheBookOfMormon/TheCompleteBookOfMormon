@echo off
dotnet ef database update --project "TheCompleteBookOfMormon.Web.Server\TheCompleteBookOfMormon.Web.Server.csproj" --startup-project "TheCompleteBookOfMormon.Web.Server\TheCompleteBookOfMormon.Web.Server.csproj" -c ApplicationDbContext
pause