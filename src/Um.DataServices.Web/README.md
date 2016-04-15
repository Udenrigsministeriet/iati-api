# Setup

1. Create the following sites in IIS:
	1. http://iatiquery.um.dk
	2. http://iatiquery-test.um.dk


# Notes

After updating the DB model, add the following line to Database.Context.cs
```
((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 3000;
```

