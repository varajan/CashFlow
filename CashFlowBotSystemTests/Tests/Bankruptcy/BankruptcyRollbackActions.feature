Feature: Bankruptcy Rollback Sell Assets

Background:
	Given I am 'Ruairi Hahn' user
		And I play as 'Car mechanic'
	When I get $30,000 as a credit

		And I buy a boat
		And I buy 10 Pesos with price $300 each
		And I buy 10 Acrs of land with price $10,000 as Big opprotunity
		And I buy 1000 shares of 'ok4u' stock with price $1 each
		And I start the Auto Tools company with $3000

		And I buy real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Small       | 2/1    |  60,000 |         3,000 |              160 |
		| Small       | 3/2    |  65,000 |         5,000 |             -100 |

		And I buy businesses:
		| Title    | Price  | First Payment | Monthly Cashflow |
		| Car wash | 20,000 |         5,000 |              800 |
	When I get a paycheck
		And I see bankruptcy message

Scenario Outline: Rollback sell action
	When I sell <asset> asset
	But I rollback last <actions> actions
	Then I see my bankruptcy message

Examples:
	| asset      | actions |
	| OK4U       |       1 |
	| Boat       |       2 |
	| Car Wash   |       2 |
	| Auto Tools |       2 |
	| Peso       |       2 |
	| 10 Acrs    |       2 |
	| 3/2        |       2 |
