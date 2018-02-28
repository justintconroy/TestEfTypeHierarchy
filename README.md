I created
[an issue for this in the EF6 repo](https://github.com/aspnet/EntityFramework6/issues/480)
to hopefully get clarification on whether or not this can be fixed.

# Introduction
A sample project demonstrating behavior of Entity Framework with
inheritance in the model. This test was made using .NET Framework v4.7
and Entity Framework v6.2.0.

In this case, there are 3 types. `BaseType`, `DerivedTypeA`, and
`DerivedTypeB`. `DerivedTypeA` inherits from `BaseType` and
`DerivedTypeB` inherits from `DerivedTypeA`, so the hierarchy is like
such:

```
DerivedTypeB->DerivedTypeA->BaseType
```

By default, this results in a Table Per Hierarchy (TPH) mapping, such that
all 3 types are saved to the same table and a `Discriminator` column
is used to disambiguate them. For the Initial migration, we start out
with just `BaseType` and `DerivedTypeA`.

# Try it for yourself
Download this project and open up the solution. Try commenting or
uncommenting various lines in the `OnModelBuilding` method and then
generating a migration to see what happens.

Generate a migration (make sure you're in the `TypeHierarchyModel`
project directory before doing this. You also may need to build in VS
first to make sure NuGet has already restored the tools).
```
dotnet ef migrations add SwitchToDerivedModels
```

There are some cases where that command actually succeeds, but creates a
less than ideal migration. If you want to try something else, don't
update your database, and just delete the generated migration files.

# What are you even trying to do?
I'm trying to entirely replace an entity with another one in the model.
That is, I'd like to pretend that `DerivedTypeA` doesn't even exist and
only map `BaseType` and `DerivedTypeB`. In my particular case,
`DerivedTypeA` also has a navigation property to another type that's
going to be completely replaced (`OtherBaseType` will be replaced by
`DerivedOtherType`).

# Why in the world are you doing that?
For... reasons. You don't need to know!

Okay, well I'll explain a bit. In my Real Project™, I actually have 2
models/database contexts. One of the models (let's call it Secondary)
overlaps with the other model (we'll call it Primary) quite a bit.
Actually, Secondary includes all the tables from Primary. Both models
make use of the same tables within the database, making Primary a sort
of common data store for multiple instances of Secondary (in separate
schemas). There are good reasons for this separation that I won't get
into here.

Some of the tables in Secondary have foreign keys to tables in Primary.
It's nice to have foreign keys and navigation properties between the
tables in Primary and Secondary (since it is actually related data).

Creating the foreign keys from the Secondary side of things is easy
enough. We just add navigation properties to the other models. But it's
also nice to have navigation properties going the other way. In order to
accomplish this, but still maintain consistency between the different
models is to inherit from those models in the Secondary model and just
add a navigation property there. When you do this, we also want to
ignore the original table rather than add a Discriminator column to
disambiguate. Again, we just want to replace the original table
completely with the derived table.

When you do this, you also need to change the navigation properties on
your Secondary models to point to the new objects instead of the old.
You also need to hide any existing navigation properties that point to
models that are being replaced by using the `new` keyword and changing
the type to your derived type. If you don't do that, EF won't complain
at all, it just won't return any data (because the stuff in the table
isn't the right "type", I guess). This actually works quite well when
you have only one level of inheritance. But occasionally you're going to
run into a case where the original models in the Primary model were
already derived from some base type (i.e. TPH is already being used for
that table and you already have a discriminator). This is where we start
to run into problems.

# The Problem
The goal here is to ignore `DerivedTypeA` (i.e. not map it to the
database at all) and still have `DerivedTypeB` and `BaseType` map to the
same table with a Discriminator.

```csharp
protected override void OnModelCreating(DbModelBuilder modelBuilder)
{
	modelBuilder.Ignore<DerivedTypeA>();
}
```

Instead, with no additional mapping, the next migration will change
your one table into two tables (i.e. switching from TPH to Table
Per Concrete Type (TPC)). Since this isn't what we want, we can try
to add add a mapping to our `OnModelCreatingMethod` to force
`BaseType` and `DerivedTypeB` into the same table.

```csharp
protected override void OnModelCreating(DbModelBuilder modelBuilder)
{
	modelBuilder.Ignore<DerivedTypeA>();

	modelBuilder.Entity<BaseType>()
		.ToTable("BaseType");
	modelBuilder.Entity<DerivedTypeB>()
		.ToTable("BaseType");
}
```

But Entity Framework doesn't like this change. If you try to create
a migration for this (or even just run your program and access those
tables since no migration should actually be needed), you get an
exception.

```
Unhandled Exception: System.InvalidOperationException: The entity types 'BaseType' and 'DerivedTypeB' cannot share table 'BaseTypes' because they are not in the same type hierarchy or do not have a valid one to one foreign key relationship with matching primary keys between them.
   at System.Data.Entity.ModelConfiguration.Configuration.Mapping.EntityMappingConfiguration.UpdateColumnNamesForTableSharing(DbDatabaseMapping databaseMapping, EntityType entityType, EntityType toTable, MappingFragment fragment)
   at System.Data.Entity.ModelConfiguration.Configuration.Mapping.EntityMappingConfiguration.FindOrCreateTargetTable(DbDatabaseMapping databaseMapping, MappingFragment fragment, EntityType entityType, EntityType fromTable, Boolean& isTableSharing)
   at System.Data.Entity.ModelConfiguration.Configuration.Mapping.EntityMappingConfiguration.Configure(DbDatabaseMapping databaseMapping, ICollection`1 entitySets, DbProviderManifest providerManifest, EntityType entityType, EntityTypeMapping& entityTypeMapping, Boolean isMappingAnyInheritedProperty, Int32 configurationIndex, Int32 configurationCount, IDictionary`2 commonAnnotations)
   at System.Data.Entity.ModelConfiguration.Configuration.Types.EntityTypeConfiguration.ConfigureTablesAndConditions(EntityTypeMapping entityTypeMapping, DbDatabaseMapping databaseMapping, ICollection`1 entitySets, DbProviderManifest providerManifest)
   at System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration.ConfigureEntityTypes(DbDatabaseMapping databaseMapping, ICollection`1 entitySets, DbProviderManifest providerManifest)
   at System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration.Configure(DbDatabaseMapping databaseMapping, DbProviderManifest providerManifest)
   at System.Data.Entity.DbModelBuilder.Build(DbProviderManifest providerManifest, DbProviderInfo providerInfo)
   at System.Data.Entity.DbModelBuilder.Build(DbConnection providerConnection)
   at System.Data.Entity.Internal.LazyInternalContext.CreateModel(LazyInternalContext internalContext)
   at System.Data.Entity.Internal.RetryLazy`2.GetValue(TInput input)
   at System.Data.Entity.Internal.LazyInternalContext.InitializeContext()
   at System.Data.Entity.Internal.LazyInternalContext.get_ModelBeingInitialized()
   at System.Data.Entity.Infrastructure.EdmxWriter.WriteEdmx(DbContext context, XmlWriter writer)
   at System.Data.Entity.Utilities.DbContextExtensions.GetModel(Action`1 writeXml)
   at System.Data.Entity.Migrations.DbMigrator..ctor(DbMigrationsConfiguration configuration, DbContext usersContext, DatabaseExistenceState existenceState, Boolean calledByCreateDatabase)
   at System.Data.Entity.Migrations.DbMigrator..ctor(DbMigrationsConfiguration configuration)
   at System.Data.Entity.Migrations.Design.MigrationScaffolder..ctor(DbMigrationsConfiguration migrationsConfiguration)
   at Migrator.EF6.Tools.Executor.AddMigration(String name, String outputDir, Boolean ignoreChanges)
   at Migrator.EF6.Tools.MigrationsCommand.<>c__DisplayClass1_2.<ConfigureInternal>b__6()
   at Microsoft.Extensions.CommandLineUtils.CommandLineApplication.Execute(String[] args)
   at Migrator.EF6.Tools.Program.Main(String[] args)
```

It would seem that entity framework no longer considers `BaseType`
and `DerivedTypeB` to be part of the same type hierarchy once you
instruct it to ignore `DerivedTypeA`.

# Observations
It's not clear whether this is a bug or actually intended behavior in
Entity Framework. I can see arguments for going either way being
reasonable.

If it's not intentional, I suppose that would mean EF is doing something
wrong when testing if two types are from the same type hierarchy.

If it's intentional, then that means we can't actually fully replace
tables that are already using TPH. At least not without making some odd
changes to your database schema (such as using a different column for
the key in the derived table even though it's the same as the base
table. Trying to force the mappings to be the same either doesn't work,
or in some cases actually throws an error, depending on what things you
explicitly define in the mapping that are in conflict.

# Workaround
Really, all you can do is either allow the "extra" foreign key column to
exist and migrate your data to that, or just switch to a TPC mapping.
The extra foreign key is likely related to
[this issue](https://github.com/aspnet/EntityFramework6/issues/443).
If that issue was fixed, we could just leave the original table alone
(i.e. don't ignore it), and the database would still look the same. We'd
just have to change the discriminator value for the new table to match
the old (and maybe change the old to something else. I haven't tested
whether EF complains about multiple types with the same Discriminator or
not).
