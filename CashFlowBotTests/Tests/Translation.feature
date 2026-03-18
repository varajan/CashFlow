Feature: Translation

Scenario Outline: I can use any language
	Given I am '<name>' user
		And I say '<details>'
		And I say '<stop>'
		And I say '<yes>'

	When I say '<lang>'
		And I say '<role>'
		And I say '<cashflow>'
		And I say '<baby>'
		And I say '<downsize>'

	Then My last message contains '*<profession>* <role>'
		And My last message contains '*<cash>* $320'
		And My last message contains '*<expenses>* $3,460'

Examples:
	| lang | name         | details      | stop          | yes | role      | cashflow       | baby   | downsize   | profession  | cash     | expenses  |
	| UA   | Тарас Тополя | Мої дані     | Закінчити гру | Так | Інженер   | Грошовий потік | Дитина | Звільнення | Професія:   | Готівка: | Витрати:  |
	| EN   | Lilli Nolan  | Show my data | Stop game     | Yes | Engineer  | Paycheck       | Baby   | Downsize   | Profession: | Cash:    | Expenses: |
	| DE   | Lukas Müller | Meine Info   | Spiel beenden | Ja  | Ingenieur | Gehalt         | Kind   | Entlassung | Beruf:      | Bargeld: | Ausgaben: |
