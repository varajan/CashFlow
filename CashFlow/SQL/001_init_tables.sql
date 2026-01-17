--DROP TABLE  IF EXISTS AvailableAssets;
CREATE TABLE IF NOT EXISTS Terms (ID Number, Language Text, Term Text);
CREATE TABLE IF NOT EXISTS Users (ID Number, Stage Text, Admin Number, Name Text, Language Text, LastActive Text, FirstLogin Text);
CREATE TABLE IF NOT EXISTS Persons (ID Number, PersonData Text);
--CREATE TABLE IF NOT EXISTS Persons (ID Number, Profession Text, Salary Number, Cash Number, ReadyForBigCircle Number, BigCircle Number, InitialCashFlow Number, Bankruptcy Number, CreditsReduced Number);
--CREATE TABLE IF NOT EXISTS Expenses (ID Number, Taxes Number, Mortgage Number, SchoolLoan Number, CarLoan Number, CreditCard Number, SmallCredits Number, BankLoan Number, Others Number, Children Number, PerChild Number);
--CREATE TABLE IF NOT EXISTS Liabilities (ID Number, Mortgage Number, SchoolLoan Number, CarLoan Number, CreditCard Number, SmallCredits Number, BankLoan Number);
--CREATE TABLE IF NOT EXISTS Assets (AssetID Number, UserID Number, Type Number, Deleted Number, Draft Number, BigCircle Number, Title Text, Price Number, Qtty Number, Mortgage Number, CashFlow Number, SellPrice Number);
CREATE TABLE IF NOT EXISTS AvailableAssets (Type Number, Language Text, Value Text, UNIQUE (Type, Language, Value));
CREATE TABLE IF NOT EXISTS History (UserID Number, Id Number, HistoryRecord Text);
--CREATE TABLE IF NOT EXISTS History (ID Number, UserID Number, ActionType Number, Value Number, Description Text);
CREATE TABLE IF NOT EXISTS DefaultPersonData (ID Number, Salary Number, Cash Number, ExpensesTaxes Number, ExpensesMortgage Number, ExpensesSchoolLoan Number, ExpensesCarLoan Number, ExpensesCreditCard Number, ExpensesOthers Number, ExpensesPerChild Number, ExpensesSmallCredits Number, LiabilitiesMortgage Number, LiabilitiesSchoolLoan Number, LiabilitiesCarLoan Number, LiabilitiesCreditCard Number, LiabilitiesSmallCredits Number);

DELETE FROM AvailableAssets;
DELETE FROM Terms;
