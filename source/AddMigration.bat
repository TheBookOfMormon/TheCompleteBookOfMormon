@echo off
dotnet ef migrations add "%1" --project "TheCompleteBookOfMormonTheCompleteBookOfMormon.Domain\TheCompleteBookOfMormon.Domain.csproj" --startup-project "TheCompleteBookOfMormon.Web\TheCompleteBookOfMormon.Web.csproj" --output-dir "Migrations" -c ApplicationDbContext
pause