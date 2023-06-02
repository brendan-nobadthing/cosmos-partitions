using Bogus;

namespace CosmosPartitions.Console;

public class Data
{

    public Data()
    {
        Randomizer.Seed = new Random(6065537);
        Organisations = BuildOrganisations();
        Products = BuildProducts();
        Users = BuildUsers();
    }
    public IList<Organisation> Organisations { get; private set; }

    private IList<Organisation> BuildOrganisations()
    {
        var orgFaker = new Faker<Organisation>("en_AU")
            .RuleFor(x => x.OrgId, f => f.Random.Guid())
            .RuleFor(x => x.OrgName, f => f.Company.CompanyName())
            .RuleFor(x => x.Address, f => f.Address.FullAddress());

        return orgFaker.Generate(200);
    }
    
    public IList<User> Users { get; private set; }
    private IList<User> BuildUsers()
    {
        var orgFaker = new Faker<User>("en_AU")
            .RuleFor(x => x.UserId, f => f.Random.Guid())
            .RuleFor(x => x.OrgId, f => f.Random.ListItem(Organisations).OrgId)
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.Address, f => f.Address.FullAddress())
            .RuleFor(x => x.State, f => f.Address.State())
            .RuleFor(x => x.Dob, f => f.Date.PastDateOnly(70, new DateOnly(2010,1,1)));

        return orgFaker.Generate(10000);
    }
    
    public IList<Product> Products { get; private set; }
    
    private IList<Product> BuildProducts()
    {
        var orgFaker = new Faker<Product>("en_AU")
            .RuleFor(x => x.ProductId, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Commerce.Product())
            .RuleFor(x => x.Department, f => f.Commerce.Department())
            .RuleFor(x => x.Price, f => f.Commerce.Price())
            .RuleFor(x => x.Barcode, f => f.Commerce.Ean13())
            .RuleFor(x => x.Color, f => f.Commerce.Color());
        
        return orgFaker.Generate(1000);
    }


    public IEnumerable<UserProductEvent> GetEvents(int count)
    {
        var eventFaker = new Faker<UserProductEvent>("en_AU")
            .RuleFor(x => x.EventType, f => f.Random.Enum<UserProductEventType>())
            .RuleFor(x => x.User, f => f.Random.ListItem(Users))
            .RuleFor(x => x.Product, f => f.Random.ListItem(Products))
            .RuleFor(x => x.User, f => f.Random.ListItem(Users))
            .RuleFor(x => x.Organisation, f => f.Random.ListItem(Organisations))
            .RuleFor(x => x.TimestampUtc, f => f.Date.Past(3, new DateTime(2023, 1, 1)))
            .RuleFor(x => x.Filler, f => f.Lorem.Paragraphs(500))
            ;
        
        for (var i = 0; i < count; i++)
        {
            var e = eventFaker.Generate(1).First();
            e.UserId = e.User!.UserId;
            e.OrgId = e.Organisation!.OrgId;
            e.ProductId = e.Product!.ProductId;
            yield return e;
        }
    }

    private UserProductEvent BuildUserProductEvent()
    {
        return new UserProductEvent();
    }
    
}



public class Organisation
{
    public Guid OrgId { get; set; }
    public string? OrgName { get; set; }
    public string? Address { get; set; }
}


public class Product
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public string? Barcode { get; set; }
    public string? Department { get; set; }
    public string? Color { get; set; }
    public string? Price { get; set; }
}


public class User
{
    public Guid UserId { get; set; }
    public Guid OrgId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public string? State { get; set; }
    public DateOnly Dob { get; set; }
}


public enum UserProductEventType 
{
    View = 1,
    AddToCart = 2,
}

public class UserProductEvent
{
    // ReSharper disable once InconsistentNaming
    public Guid id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid OrgId { get; set; }

    public Guid ProductId { get; set; }

    public UserProductEventType EventType { get; set; }
    public User? User { get; set; }
    public Product? Product { get; set; }
    public Organisation? Organisation { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? Filler { get; set; }
}

