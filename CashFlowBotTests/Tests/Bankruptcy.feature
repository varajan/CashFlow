Feature: Bankruptcy

# sell something and survive
# sell everything and loose

Scenario: I have to sell my assets when I can't pay my bills
	Given I am 'Sami Parker' user
		And I play as 'Car mechanic'
	When I get $30,000 as a credit

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

	But I get a paycheck
	Then My last message is:
"""
*You're out of money.*
Bank Loan: *$30,000*
Cashflow: *-$1,420*
Cash: *$1,390*

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
