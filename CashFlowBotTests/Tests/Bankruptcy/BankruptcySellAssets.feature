Feature: Bankruptcy Sell Assets

Background:
	Given I am 'Sami Parker' user
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

Scenario: I have to sell my assets to survive
	Then My last message is:
"""
*You're out of money.*
Bank Loan: *$30,000*
Cashflow: *-$1,760*
Cash: *$390*

You have to sell your assets till you cash flow is positive.

What asset do you want to sell?

#1 - *OK4U* - Price: $500
#2 - *Car Wash* - Price: $2,500, Cashflow: $800
#3 - *Boat* - Price: $500, Cashflow: -$340
#4 - *Auto Tools* - Price: $1,500
#5 - *Peso* - Price: $1,500
#6 - *10 Acrs* - Price: $5,000
#7 - *2/1* - Price: $1,500, Cashflow: $160
#8 - *3/2* - Price: $2,500, Cashflow: -$100
"""

Scenario: I sell shares to survive
	When I sell OK4U asset
	Then My last message is:
"""
*You're out of money.*
Bank Loan: *$30,000*
Cashflow: *-$1,760*
Cash: *$890*

You have to sell your assets till you cash flow is positive.

What asset do you want to sell?

#1 - *Car Wash* - Price: $2,500, Cashflow: $800
#2 - *Boat* - Price: $500, Cashflow: -$340
#3 - *Auto Tools* - Price: $1,500
#4 - *Peso* - Price: $1,500
#5 - *10 Acrs* - Price: $5,000
#6 - *2/1* - Price: $1,500, Cashflow: $160
#7 - *3/2* - Price: $2,500, Cashflow: -$100
"""

Scenario: I sell business to survive
	When I sell Car Wash asset
	Then My last message is:
"""
*You're out of money.*
Bank Loan: *$28,000*
Cashflow: *-$2,360*
Cash: *$890*

You have to sell your assets till you cash flow is positive.

What asset do you want to sell?

#1 - *OK4U* - Price: $500
#2 - *Boat* - Price: $500, Cashflow: -$340
#3 - *Auto Tools* - Price: $1,500
#4 - *Peso* - Price: $1,500
#5 - *10 Acrs* - Price: $5,000
#6 - *2/1* - Price: $1,500, Cashflow: $160
#7 - *3/2* - Price: $2,500, Cashflow: -$100
"""

Scenario: I sell Boat to survive
	When I sell Boat asset
	Then My last message is:
"""
*You're out of money.*
Bank Loan: *$30,000*
Cashflow: *-$1,420*
Cash: *$890*

You have to sell your assets till you cash flow is positive.

What asset do you want to sell?

#1 - *OK4U* - Price: $500
#2 - *Car Wash* - Price: $2,500, Cashflow: $800
#3 - *Auto Tools* - Price: $1,500
#4 - *Peso* - Price: $1,500
#5 - *10 Acrs* - Price: $5,000
#6 - *2/1* - Price: $1,500, Cashflow: $160
#7 - *3/2* - Price: $2,500, Cashflow: -$100
"""

Scenario: I sell small business to survive
	When I sell Auto Tools asset
	Then My last message is:
"""
*You're out of money.*
Bank Loan: *$29,000*
Cashflow: *-$1,660*
Cash: *$890*

You have to sell your assets till you cash flow is positive.

What asset do you want to sell?

#1 - *OK4U* - Price: $500
#2 - *Car Wash* - Price: $2,500, Cashflow: $800
#3 - *Boat* - Price: $500, Cashflow: -$340
#4 - *Peso* - Price: $1,500
#5 - *10 Acrs* - Price: $5,000
#6 - *2/1* - Price: $1,500, Cashflow: $160
#7 - *3/2* - Price: $2,500, Cashflow: -$100
"""

Scenario: I sell coins to survive
	When I sell Peso asset
	Then My last message is:
"""
*You're out of money.*
Bank Loan: *$29,000*
Cashflow: *-$1,660*
Cash: *$890*

You have to sell your assets till you cash flow is positive.

What asset do you want to sell?

#1 - *OK4U* - Price: $500
#2 - *Car Wash* - Price: $2,500, Cashflow: $800
#3 - *Boat* - Price: $500, Cashflow: -$340
#4 - *Auto Tools* - Price: $1,500
#5 - *10 Acrs* - Price: $5,000
#6 - *2/1* - Price: $1,500, Cashflow: $160
#7 - *3/2* - Price: $2,500, Cashflow: -$100
"""

Scenario: I sell land to survive
	When I sell 10 Acrs asset
	Then My last message is:
"""
*You're out of money.*
Bank Loan: *$25,000*
Cashflow: *-$1,260*
Cash: *$390*

You have to sell your assets till you cash flow is positive.

What asset do you want to sell?

#1 - *OK4U* - Price: $500
#2 - *Car Wash* - Price: $2,500, Cashflow: $800
#3 - *Boat* - Price: $500, Cashflow: -$340
#4 - *Auto Tools* - Price: $1,500
#5 - *Peso* - Price: $1,500
#6 - *2/1* - Price: $1,500, Cashflow: $160
#7 - *3/2* - Price: $2,500, Cashflow: -$100
"""

Scenario: I sell real estate to survive
	When I sell 3/2 asset
	Then My last message is:
"""
*You're out of money.*
Bank Loan: *$28,000*
Cashflow: *-$1,460*
Cash: *$890*

You have to sell your assets till you cash flow is positive.

What asset do you want to sell?

#1 - *OK4U* - Price: $500
#2 - *Car Wash* - Price: $2,500, Cashflow: $800
#3 - *Boat* - Price: $500, Cashflow: -$340
#4 - *Auto Tools* - Price: $1,500
#5 - *Peso* - Price: $1,500
#6 - *10 Acrs* - Price: $5,000
#7 - *2/1* - Price: $1,500, Cashflow: $160
"""

Scenario: I sell everything to survive
	When I sell all 8 assets
	Then My last message is: You are bankrupt. Game is over.
