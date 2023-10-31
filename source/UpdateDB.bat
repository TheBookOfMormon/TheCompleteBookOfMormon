@echo off
dotnet ef database update --project "TheCompleteBookOfMormon.Domain\TheCompleteBookOfMormon.Domain.csproj" --startup-project "TheCompleteBookOfMormon.Web\TheCompleteBookOfMormon.Web.csproj" -c ApplicationDbContext
pause