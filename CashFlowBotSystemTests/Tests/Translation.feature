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
	| lang | name         | details      | stop          | yes | role      | cashflow       | baby   | downsize   | profession  | cash     | expenses  |
	| UA   | Тарас Тополя | Мої дані     | Закінчити гру | Так | Інженер   | Грошовий потік | Дитина | Звільнення | Професія:   | Готівка: | Витрати:  |
	| EN   | Lilli Nolan  | Show my data | Stop game     | Yes | Engineer  | Paycheck       | Baby   | Downsize   | Profession: | Cash:    | Expenses: |
	| DE   | Lukas Müller | Meine Info   | Spiel beenden | Ja  | Ingenieur | Gehalt         | Kind   | Entlassung | Beruf:      | Bargeld: | Ausgaben: |


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
	| lang | new lang | name           | game menu | change lang        | details      | stop          | yes | role      | profession             | cash     | expenses  |
	| UA   | EN       | Іван Бондар    | Гра       | Language/Мова      | Мої дані     | Закінчити гру | Так | Інженер   | *Profession:* Engineer | Cash:    | Expenses: |
	| EN   | DE       | Billie Craig   | Game menu | Language/Мова      | Show my data | Stop game     | Yes | Engineer  | *Beruf:* Ingenieur     | Bargeld: | Ausgaben: |
	| DE   | UA       | Ursula Schäfer | Spielmenu | Schprache/Language | Meine Info   | Spiel beenden | Ja  | Ingenieur | *Професія:* Інженер    | Готівка: | Витрати:  |

