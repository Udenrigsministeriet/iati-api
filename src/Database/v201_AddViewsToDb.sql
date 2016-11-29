USE [IATIv201]
GO

/****** Object:  View [dbo].[Activity]    Script Date: 23-11-2016 09:07:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE view [dbo].[Activity] as
select 
a.ProjectId,
a.[iati-activityID],
[@last-updated-datetime] LastUpdateDateTime,
[iati-identifier/text()] as IatiIdentifier,
[other-identifier/text()] as JournalNo,
[@hierarchy] as LevelNo,
at.[text()] as Title,
[activity-status/text()] as ActivityStatus,
--[activity-date/start-planned/text()] as DateStartPlanned,
--[activity-date/end-planned/text()] as DateEndPlanned,
--[activity-date/start-actual/text()] as DateStartActual,
--[activity-date/end-actual/text()] as DateEndActual,
cid.[email/text()] as ContactInfoEmail,
ci.[organisation/text()] as ContactInfoOrganisation,
ci.[person-name/text()] as ContactInfoPerson,
cid.[telephone/text()] as ContactInfoTelephone,
b.[text()] as [Description]
from IATISchema.[iati-activity] a
left join IATISchema.[activity/title] at on at.[iati-activityID] = a.[iati-activityID]
left join IATISchema.[activity/description] b on a.[iati-activityID] = b.[iati-activityID]
left join IATISchema.[contact-info] ci on ci.[iati-activityID] = a.[iati-activityID]
left join IATISchema.[contact-info/details] cid on cid.[contact-infoID] = ci.[contact-infoID]
and a.[iati-activityId] = b.[iati-activityID] and b.[@type] = 1

GO

CREATE VIEW [dbo].[Activity_Budget] as
SELECT 
b.budgetID 
,b.[iati-activityID]
,[value/@currency] as CurrencyCode
,[value/@value-date] as BudgetDate
,[value/text()] as BudgetAmount
,uc.country_code as CountryCode
,uc.country_code_iati as CountryCodeIati
,org.[text()] AS OrganisationId
,sec.[@code] AS SectorCode
,flov.[text()] AS SubcompanyCode
,reg.[@code] AS RegionCode
,country.name_uk as RegionName
,uc.name_uk as CountryName
,umorg.organisation_name_uk as OrganisationName
,sector_name_uk as SectorName
,finanslov.BudgetSubSection
FROM IATISchema.[budget] b
LEFT JOIN [IATISchema].[recipient-country] c ON b.[iati-activityID] = c.[iati-activityID]
LEFT JOIN um.Country uc ON c.[@code] = uc.country_code_iati
LEFT JOIN iatischema.[participating-org] ch ON b.[iati-activityID] = ch.[iati-activityID] AND ch.[@role] = 'Implementing'
LEFT JOIN [IATISchema].[other-identifier] org ON b.[iati-activityID] = org.[ActivityId] AND [@owner-name] = 'Dk-org'
LEFT JOIN [IATISchema].[sector] sec ON b.[iati-activityID] = sec.[iati-activityID]
LEFT JOIN [IATISchema].[other-identifier] flov ON b.[iati-activityID] = flov.[ActivityId] AND flov.[@owner-name] = 'Dk-SubCompany'
LEFT JOIN [iatischema].[recipient-region] reg ON b.[iati-activityID]= reg.[iati-activityID]
left join UM.Country country on reg.[@code] = country.code and country.CountryType = 'Region'
left join UM.Organisation umorg on org.[text()] = umorg.organisation_code 
left join UM.Sector sector on sec.[@code] = sector.sector_code 
left join UM.Finanslov finanslov on finanslov.SubCompanyCode = flov.[text()]

GO

/****** Script for SelectTopNRows command from SSMS  ******/
CREATE VIEW [dbo].[Activity_Transactions] AS
SELECT 
t.transactionID,
t.[iati-activityID],
t.[value/@currency] as CurrencyCode,
t.[value/@value-date] as TransactionDate,
t.[value/text()] as TransactionAmount,
t.[transaction-type/@code] as TransactionTypeCode,
t.[flow-type/@code] as FlowTypeCode,
t.[aid-type/@code] as AidType,
t.[finance-type/@code] as FinanceTypeCode,
t.[tied-status/@code] as TiedStatusCode,
t.[disbursement-channel/@code] as DisbursementChannelCode,
uc.country_code as CountryCode, 
uc.country_code_iati as CountryCodeIati, 
ch.[@ref] as ChannelCode, 
org.[text()] AS OrganisationId, 
sec.[@code] AS SectorCode, 
flov.[text()] AS SubcompanyCode, 
reg.[@code] AS RegionCode,
country.name_uk as RegionName
,uc.name_uk as CountryName
,umorg.organisation_name_uk as OrganisationName
,sector_name_uk as SectorName
,finanslov.BudgetSubSection
,channel.channel_name_uk as ChannelName
FROM IATISchema.[transaction] t
LEFT JOIN [IATISchema].[recipient-country] c ON t.[iati-activityID] = c.[iati-activityID]
LEFT JOIN um.Country uc ON c.[@code] = uc.country_code_iati
LEFT JOIN iatischema.[participating-org] ch ON t.[iati-activityID] = ch.[iati-activityID] AND ch.[@role] = 'Implementing'
LEFT JOIN [IATISchema].[other-identifier] org ON t.[iati-activityID] = org.[ActivityId] AND [@owner-name] = 'Dk-org'
LEFT JOIN [IATISchema].[sector] sec ON t.[iati-activityID] = sec.[iati-activityID]
LEFT JOIN [IATISchema].[other-identifier] flov ON t.[iati-activityID] = flov.[ActivityID] AND flov.[@owner-name] = 'Dk-SubCompany'
LEFT JOIN [iatischema].[recipient-region] reg ON t.[iati-activityID]= reg.[iati-activityID]
left join UM.Country country on reg.[@code] = country.code and country.CountryType = 'Region'
left join UM.Organisation umorg on org.[text()] = umorg.organisation_code 
left join UM.Sector sector on sec.[@code] = sector.sector_code 
left join UM.Finanslov finanslov on finanslov.SubCompanyCode = flov.[text()]
left join UM.Channel channel on ch.[@ref] = channel.channel_code

GO

/****** Script for SelectTopNRows command from SSMS  ******/
create view [dbo].[Aidtype] as
SELECT 
       [ID]
	  ,[aidtype_code]
      ,[aidtype_name_uk]
      ,[parent_code]
      ,[parent_name_uk]
      ,[aidtype_name_dk]
      ,[parent_name_dk]
  FROM [IATIv201].[UM].[Aidtype]

GO

/****** Script for SelectTopNRows command from SSMS  ******/
CREATE view [dbo].[Channel] as
SELECT 
       [ID]
	  ,[channel_code]
      ,[channel_category_code]
      ,[channel_name_uk]
      ,[channel_name_dk]
  FROM [IATIv201].[UM].[Channel]

GO

/****** Script for SelectTopNRows command from SSMS  ******/
CREATE view [dbo].[Country] as
SELECT 
       [ID]
	  ,[country_code]
      ,[name_uk]
      ,[name_dk]
      ,[parent_code]
	  ,[country_code_iati]
	  ,[Code]
	  ,[Countrytype]
	  ,[partner_country_yn]
  FROM [IATIv201].[UM].[Country]

GO

/****** Script for SelectTopNRows command from SSMS  ******/
create view [dbo].[Currency] as
SELECT 
       [ID]
	  ,[Year]
      ,[CurrencyCode]
      ,[ExchRate]
  FROM [IATIv201].[UM].[Currency]

GO

/****** Script for SelectTopNRows command from SSMS  ******/
create view [dbo].[Finanslov] as
SELECT 
       [ID] 
	  ,[SubCompanyCode]
      ,[BudgetSubSection]
      ,[BudgetSubSection_Name_UK]
      ,[BudgetSubSection_Name_DK]
      ,[BudgetSection]
      ,[BudgetSection_Name_DK]
      ,[BudgetSection_Name_UK]
      ,[BudgetGroup]
      ,[BudgetGroup_Name_UK]
      ,[BudgetGroup_Name_DK]
      ,[Category1_DK]
      ,[Category1_UK]
      ,[Category2_DK]
      ,[Category2_UK]
  FROM [IATIv201].[UM].[Finanslov]

GO

/****** Script for SelectTopNRows command from SSMS  ******/
CREATE view [dbo].[Organisation] as
SELECT 
       [ID]
	  ,[organisation_code]
      ,[organisation_name_uk]
      ,[organisation_name_dk]
	  ,[channel_code]
  FROM [IATIv201].[UM].[Organisation]

GO

CREATE view [dbo].[Region] as 
select distinct id, code, name_uk
from um.country where countrytype <> 'Country'

GO

/****** Script for SelectTopNRows command from SSMS  ******/
create view [dbo].[Sector] as 
SELECT 
       [ID]
	  ,[sector_code]
      ,[sector_name_uk]
      ,[category_code]
      ,[category_name_uk]
      ,[parent_code]
      ,[parent_name_uk]
      ,[sector_name_dk]
      ,[category_name_dk]
      ,[parent_name_dk]
  FROM [IATIv201].[UM].[Sector]

GO





