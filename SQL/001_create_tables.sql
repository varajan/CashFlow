CREATE TABLE IF NOT EXISTS Terms (ID Number, Language Text, Term Text);
CREATE TABLE IF NOT EXISTS Users (ID Number, Stage Number, Admin Number, Name Text, Language Text, LastActive Text, FirstLogin Text);
CREATE TABLE IF NOT EXISTS Persons (ID Number, Profession Text, Salary Number, Cash Number, SmallRealEstate Number, ReadyForBigCircle Number, BigCircle Number, InitialCashFlow Number, Bankruptcy Number);
CREATE TABLE IF NOT EXISTS Expenses (ID Number, Taxes Number, Mortgage Number, SchoolLoan Number, CarLoan Number, CreditCard Number, SmallCredits Number, BankLoan Number, Others Number, Children Number, PerChild Number);
CREATE TABLE IF NOT EXISTS Liabilities (ID Number, Mortgage Number, SchoolLoan Number, CarLoan Number, CreditCard Number, SmallCredits Number, BankLoan Number);
CREATE TABLE IF NOT EXISTS Assets (AssetID Number, UserID Number, Type Number, Deleted Number, Draft Number, BigCircle Number, Title Text, Price Number, Qtty Number, Mortgage Number, CashFlow Number, SellPrice Number);
CREATE TABLE IF NOT EXISTS AvailableAssets (Type Number, Value Text, UNIQUE (Type, Value));
CREATE TABLE IF NOT EXISTS History (ID Number, UserID Number, ActionType Number, Value Number, Description Text);

DELETE FROM Terms;
