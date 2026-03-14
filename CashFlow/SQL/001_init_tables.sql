CREATE TABLE IF NOT EXISTS Terms (ID Number, Language Text, Term Text);
CREATE TABLE IF NOT EXISTS Users (ID Number, Data Text);
CREATE TABLE IF NOT EXISTS Persons (ID Number, PersonData Text);
CREATE TABLE IF NOT EXISTS AvailableAssets (Type Number, Language Text, Value Text, UNIQUE (Type, Language, Value));
CREATE TABLE IF NOT EXISTS History (UserID Number, Id Number, HistoryRecord Text);
CREATE TABLE IF NOT EXISTS DefaultPersonData (ID Number, Salary Number, Cash Number, ExpensesTaxes Number, ExpensesMortgage Number, ExpensesSchoolLoan Number, ExpensesCarLoan Number, ExpensesCreditCard Number, ExpensesOthers Number, ExpensesPerChild Number, ExpensesSmallCredits Number, LiabilitiesMortgage Number, LiabilitiesSchoolLoan Number, LiabilitiesCarLoan Number, LiabilitiesCreditCard Number, LiabilitiesSmallCredits Number);

DELETE FROM AvailableAssets;
DELETE FROM Terms;
