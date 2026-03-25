Feature: Bankruptcy with boat

Background:
	Given I am 'Annabel Buckley' user
		And I play as 'Janitor'
	When I buy a boat
		And I get 17,000 as a credit
		And I pay off my Boat Loan
		And I get a paycheck


Scenario: Bankruptcy with a boat
	Then My last message is:
"""
*You're out of money.*
Bank Loan: *$17,000*
Cashflow: *-$1,050*
Cash: *$210*

You have to sell your assets till you cash flow is positive.

What asset do you want to sell?

#1 - *Boat* - Price: $9,000
"""

Scenario: I sell Boat to survive
	When I sell Boat asset
	Then My last message is: You are bankrupt. Game is over.
	And My history data is following:
"""
• Buy a boat: $18,000
• Get credit: $17,000
• Reduce Liabilities. Boat Loan: $17,000
• Bankruptcy
• Sale for debts. *Boat* - Price: $18,000
• Reduce Liabilities. Bank Loan: $9,000
• Debt restructuring
"""

Scenario: I rollback sell Boat action
	When I see bankruptcy message
	And I sell Boat asset
	But I rollback last 3 actions
	Then I see my bankruptcy message

