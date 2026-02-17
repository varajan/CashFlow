Feature: Friends

Scenario: I can see my friends
	Given Few players:
	| Name              | Profession |
	| Bronwyn Berry     | Doctor     |
	| Damien Washington | Pilot      |
	| Bryony Morrison   | Teacher    |
	| Kelsie Humphrey   | Lawyer     |

	When Bronwyn Berry get $1000 as a credit
		And Damien Washington get $1000 in cash
		And Bryony Morrison get $10000 in cash
		And Bryony Morrison buys real estate:
		| Opportunity | Title  | Price   | First Payment | Monthly Cashflow |
		| Small       | 2/1    |  60,000 |         2,000 |            1,000 |
		| Big         | 8-plex | 150,000 |         5,000 |            1,500 |
		And Bryony Morrison goes to the Big Circle

	Then Kelsie Humphrey can see friends:
"""
*On Small circle:*
• Bronwyn Berry
• Damien Washington

*On Big circle:*
• Bryony Morrison
"""
