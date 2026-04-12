Feature: Translation

Scenario Outline: I can use any language
	Given I am '<name>' user
		And I say '<lang>'
		And I say '<role>'

	When I say '<cashflow>'
		And I say '<baby>'
		And I say '<downsize>'

	Then My last message contains '*<profession>* <role>'
		And My last message contains '*<cash>* $320'
		And My last message contains '*<expenses>* $3,460'

Examples:
	| lang | name         | role      | cashflow       | baby   | downsize   | profession  | cash     | expenses  |
	| UA   | Тарас Тополя | Інженер   | Грошовий потік | Дитина | Звільнення | Професія:   | Готівка: | Витрати:  |
	| EN   | Lilli Nolan  | Engineer  | Paycheck       | Baby   | Downsize   | Profession: | Cash:    | Expenses: |
	| DE   | Lukas Müller | Ingenieur | Gehalt         | Kind   | Entlassung | Beruf:      | Bargeld: | Ausgaben: |


Scenario Outline: I can change language
	Given I am '<name>' user
		And I say '<lang>'
		And I say '<role>'

	When I say '<game menu>'
		And I say '<change lang>'
		And I say '<new lang>'

	Then My last message contains '<profession>'
		And My last message contains '*<cash>* $2,090'
		And My last message contains '*<expenses>* $3,210'

Examples:
	| lang | new lang | name           | game menu | change lang        | role      | profession             | cash     | expenses  |
	| UA   | EN       | Іван Бондар    | Меню гри  | Language/Мова      | Інженер   | *Profession:* Engineer | Cash:    | Expenses: |
	| EN   | DE       | Billie Craig   | Game menu | Language/Мова      | Engineer  | *Beruf:* Ingenieur     | Bargeld: | Ausgaben: |
	| DE   | UA       | Ursula Schäfer | Spielmenü | Schprache/Language | Ingenieur | *Професія:* Інженер    | Готівка: | Витрати:  |

