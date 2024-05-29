@echo off
dotnet ef migrations add "%1" --project "TheCompleteBookOfMormon.Web.Server\TheCompleteBookOfMormon.Web.Server.csproj" --startup-project "TheCompleteBookOfMormon.Web.Server\TheCompleteBookOfMormon.Web.Server.csproj" --output-dir "Migrations" -c ApplicationDbContext
pause