# PostgreSQL and MongoDB benchmark based on Eurostat data
## Prerequisites
* Download a CSV from [here](https://ec.europa.eu/eurostat/databrowser/view/bop_euins6_m/default/table) in SDMX-CSV format (the expected file name is bop_euins6_m_linear.csv).
* Set up an empty database for the benchmark on your MongoDB and PostgreSQL servers.
* Populate `MongoDbConnectionString` and `PostgreSqlConnectionString` connection string parameters in all `ScenarioParameters*` files (in the future the tool is to be enhanced to reduce copy-paste by having parameter includes).
* Modify `BENCHMARK_TOOL_PATH` variable in `bop_variables.sh` if necessary.

## Benchmark scripts
*The scripts were written for a Linux environment but can be easily adapted for Windows.*

* `bop_create.sh` - creates empty tables in PostgreSQL and collections in MongoDB. After running these scripts it is necessary to create all relevant indexes:
```
//MongoDB
db.BOP_CompositeIdx.createIndex({ "PartnerCountry": 1, "TimePeriod": 1 });
db.BOP_SeparateIdx.createIndex({ "PartnerCountry": 1 });
db.BOP_SeparateIdx.createIndex({ "TimePeriod": 1 });

//PostgreSQL
CREATE INDEX bop_compositeidx_countryperiod_idx
ON public.bop_compositeidx (partnercountry, timeperiod);

CREATE INDEX bop_jsonb_compositeidx_countryperiod_idx
ON public.bop_jsonb_compositeidx 
((attributes->>'PartnerCountry'::text), (attributes->>'TimePeriod'::text));

CREATE INDEX bop_separateidx_country_idx
ON public.bop_separateidx (partnercountry);

CREATE INDEX bop_separateidx_timeperiod_idx
ON public.bop_separateidx (timeperiod);
```
* `bop_insert.sh` - runs the insert benchmark. The results can be found in `_insert.csv` file. Currently, the file must be edited after the test to have a valid CSV, because the script dumps raw output from multiple benchmark runs that may also have some warnings.
* `bop_queries.sh` - runs all query benchmarks. Must be run after the insert benchmark to have all tables and collection populated with the test data. Each individual benchmark run will produce a valid CSV file.