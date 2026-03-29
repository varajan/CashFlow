CREATE TABLE IF NOT EXISTS Terms (ID Number, Language Text, Term Text);
CREATE TABLE IF NOT EXISTS Users (ID Number, Data Text);
CREATE TABLE IF NOT EXISTS Persons (ID Number, PersonData Text);
CREATE TABLE IF NOT EXISTS AvailableAssets (Type Number, Language Text, Value Text, UNIQUE (Type, Language, Value));
CREATE TABLE IF NOT EXISTS History (UserID Number, Id Number, HistoryRecord Text);

DELETE FROM AvailableAssets;
DELETE FROM Terms;
