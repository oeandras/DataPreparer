# DataPreparer
Cleans up data quality errors in large CSV files

FIRST COMMIT:

DataPreparer contains a C# code that removes duplicate data in the .tsv files and modifies the .json metadata accordingly.
Note that the filepaths have to be set according to where your data is.

visualisierungphp contains the php script required for a webinterface. The schema creation works, but the data upload part is not yet ready.
In the schema there is a constants index and indexes for each dataset. The constants has 3 fields, one of these store which dataset the constant belongs to, the other 2 are the constant id and value.
Identifier datatype from the .json files was changed to text datatype when creating the schema.

SECOND COMMIT:

Added JSON Parser to DataPreparer, txt output now contains column names as well.

PHP upload seems good, though for some reason the memory_limit in code didn't do much, maybe changing to .ini file can help.
Constants are now stored with the other data in the database.

Also tidied up the repo, deleting unnecessary files.

THIRD COMMIT:

First working version, with requests through javascript.
Json and tsv files have to be in different folders, an nothing else should be in these respective folders.

What remains to be done:
More informative, cleaner html page,
Either fixing the data during the Data preparation where there are empty values, or deleting these missions, as any bulk that has a row where there is empty value and Elasticsearch cannot parse it (like date) makes that bulk upload fail. I'll either fix this problem or collect the name of the missions where this can be a problem.

FOURTH COMMIT:

HTML now gives information about what stage it is at.
Rows in the TSV files where empty values are found are deleted, this results in a lot of deleted lines, in fact, some TSV files are completely flushed out, but there's still a lot of data to work with, which won't have empty value problems.
Because of this, there is no more problem with bulk uploads failing because of empty values, neither PHP nor Elasticsearch threw any error.
Finally, in the third commit I slightly modified the CSVReader class, but forgot to add it to the GIT, this is now included.

FINAL COMMIT:

Visualization is now complete, couldn't do backtrack part because neither a new canvas or chartjs.destroy() worked, but everything can be done with refreshing the page.