using System;
using System.Collections.Generic;
using InsightApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

public partial class InsightUpdateCvgs2Context : IdentityDbContext<Account, AccountRole, Guid>
{
    private readonly bool _isTesting;
    public InsightUpdateCvgs2Context()
    {
    }

    public InsightUpdateCvgs2Context(DbContextOptions<InsightUpdateCvgs2Context> options, bool isTesting = false)
        : base(options)
    {
        _isTesting = isTesting;
    }

    public virtual DbSet<Country> Country { get; set; }
    public virtual DbSet<Province> Province { get; set; }
    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AddressTable> AddressTables { get; set; }
    public virtual DbSet<EventAddressTable> EventAddressTables { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<Friend> Friends { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameCategory> GameCategories { get; set; }

    public virtual DbSet<GameDetailsCategory> GameDetailsCategories { get; set; }

    public virtual DbSet<GameDetailsLanguage> GameDetailsLanguages { get; set; }

    public virtual DbSet<GameDetailsPlatform> GameDetailsPlatforms { get; set; }

    public virtual DbSet<GameEvent> GameEvents { get; set; }

    public virtual DbSet<GamePlatform> GamePlatforms { get; set; }

    public virtual DbSet<LanguageTable> LanguageTables { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MemberEventRegist> MemberEventRegists { get; set; }

    public virtual DbSet<MemberGameCategoryPref> MemberGameCategoryPrefs { get; set; }

    public virtual DbSet<MemberLanguagePref> MemberLanguagePrefs { get; set; }

    public virtual DbSet<MemberPlatformPref> MemberPlatformPrefs { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderTable> OrderTables { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ReviewStatus> ReviewStatuses { get; set; }

    public virtual DbSet<WishList> WishLists { get; set; }

    public virtual DbSet<GameRating> GameRatings { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<OwnedGame> OwnedGames { get; set; }

    public virtual DbSet<GameAverageRating> GameAverageRatings { get; set; }
    public virtual DbSet<GameRatingReport> GameRatingReports { get; set; }
    public virtual DbSet<MemberListReport> MemberListReports { get; set; }
    public virtual DbSet<SalesReport> SalesReports { get; set; }
    public virtual DbSet<WishListReport> WishListReports { get; set; }
    public virtual DbSet<EventsRegistrationsReport> EventsRegistrationsReports { get; set; }
    public virtual DbSet<MemberOrderDetailsReport> MemberOrderDetailsReports { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("name=SVGSContext");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).IsRequired().HasConversion(
                guid => guid.ToString(), 
                str => Guid.Parse(str));
        });

        modelBuilder.Entity<AddressTable>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__AddressT__091C2AFBF6A3594E");

            entity.Property(e => e.City).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.DelivaryInstructions).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.IsShipping).HasDefaultValue(false);
            entity.Property(e => e.MemberId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.PostalCode).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Province).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Unit).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Member).WithMany(p => p.AddressTables).HasConstraintName("FK__AddressTa__Membe__47DBAE45");
        });

        modelBuilder.Entity<EventAddressTable>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__EventAdd__091C2AFB96CC631C");

            entity.Property(e => e.City).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Country).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.PostalCode).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Province).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StreetName).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StreetNumber).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Unit).HasDefaultValueSql("(NULL)");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04F117D6BD7AD");

            entity.HasOne(d => d.Account).WithOne(p => p.Employee)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employee__Accoun__5AEE82B9");
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.HasKey(e => e.EvTypeId).HasName("PK__EventTyp__AB03639D2D71C2F2");
        });

        modelBuilder.Entity<Friend>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Friend__3213E83F008E7B68");

            entity.HasOne(d => d.FriendNavigation).WithMany(p => p.FriendFriendNavigations).HasConstraintName("FK__Friend__FriendId__114A936A");

            entity.HasOne(d => d.Member).WithMany(p => p.FriendMembers).HasConstraintName("FK__Friend__MemberId__10566F31");
        });


        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("PK__Game__2AB897FDF4196EF1");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PhysicalAvailable).HasDefaultValue(false);
            entity.Property(e => e.Price).HasDefaultValue(0.0);
        });

        modelBuilder.Entity<GameCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__GameCate__19093A0B4E9639EC");
        });

        modelBuilder.Entity<GameDetailsCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GameDeta__3213E83F5457DD6B");

            entity.HasOne(d => d.Category).WithMany(p => p.GameDetailsCategories).HasConstraintName("FK__GameDetai__Categ__7E37BEF6");

            entity.HasOne(d => d.Game).WithMany(p => p.GameDetailsCategories).HasConstraintName("FK__GameDetai__GameI__7D439ABD");
        });

        modelBuilder.Entity<GameDetailsLanguage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GameDeta__3213E83F71125F0A");

            entity.HasOne(d => d.Game).WithMany(p => p.GameDetailsLanguages).HasConstraintName("FK__GameDetai__GameI__04E4BC85");

            entity.HasOne(d => d.Language).WithMany(p => p.GameDetailsLanguages).HasConstraintName("FK__GameDetai__Langu__05D8E0BE");
        });

        modelBuilder.Entity<GameDetailsPlatform>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GameDeta__3213E83F7761C27F");

            entity.HasOne(d => d.Game).WithMany(p => p.GameDetailsPlatforms).HasConstraintName("FK__GameDetai__GameI__01142BA1");

            entity.HasOne(d => d.Platform).WithMany(p => p.GameDetailsPlatforms).HasConstraintName("FK__GameDetai__Platf__02084FDA");
        });

        modelBuilder.Entity<GameEvent>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__GameEven__7944C81050BC91AB");

            entity.Property(e => e.Duration).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.EndDate).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.EventLink).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Address).WithMany(p => p.GameEvents).HasConstraintName("FK_GameEvent_EventAddress");

            entity.HasOne(d => d.EvType).WithMany(p => p.GameEvents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GameEvent__EvTyp__5629CD9C");
        });

        modelBuilder.Entity<GamePlatform>(entity =>
        {
            entity.HasKey(e => e.PlatformId).HasName("PK__GamePlat__F559F6FA7D278275");
        });

        modelBuilder.Entity<LanguageTable>(entity =>
        {
            entity.HasKey(e => e.LanguageId).HasName("PK__Language__B93855AB20176444");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Member__0CF04B181C80A0AD");

            entity.Property(e => e.Dob).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Gender).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.PhoneNumber).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.RecievesEmails).HasDefaultValue(true);

            entity.HasOne(d => d.Account).WithOne(p => p.Member)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Member__AccountI__3E52440B");
        });

        modelBuilder.Entity<MemberEventRegist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MemberEv__3213E83FDAAA1180");

            entity.HasOne(d => d.Event).WithMany(p => p.MemberEventRegists).HasConstraintName("FK__MemberEve__Event__0D7A0286");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberEventRegists).HasConstraintName("FK__MemberEve__Membe__0C85DE4D");
        });

        modelBuilder.Entity<MemberGameCategoryPref>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MemberGa__3213E83FDBA00F49");

            entity.HasOne(d => d.Category).WithMany(p => p.MemberGameCategoryPrefs).HasConstraintName("FK__MemberGam__Categ__76969D2E");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberGameCategoryPrefs).HasConstraintName("FK__MemberGam__Membe__75A278F5");
        });

        modelBuilder.Entity<MemberLanguagePref>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MemberLa__3213E83F53AE5ADF");

            entity.HasOne(d => d.Language).WithMany(p => p.MemberLanguagePrefs).HasConstraintName("FK__MemberLan__Langu__7A672E12");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberLanguagePrefs).HasConstraintName("FK__MemberLan__Membe__797309D9");
        });

        modelBuilder.Entity<MemberPlatformPref>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MemberPl__3213E83F09299873");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberPlatformPrefs).HasConstraintName("FK__MemberPla__Membe__71D1E811");

            entity.HasOne(d => d.Platform).WithMany(p => p.MemberPlatformPrefs).HasConstraintName("FK__MemberPla__Platf__72C60C4A");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderIte__3213E83F0E19F077");

            entity.Property(e => e.IsPhysical).HasDefaultValue(0);

            entity.HasOne(d => d.Game).WithMany(p => p.OrderItems).HasConstraintName("FK__OrderItem__GameI__6EF57B66");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasConstraintName("FK__OrderItem__Order__6E01572D");
            
            entity.HasOne(d => d.Member).WithMany(p => p.OrderItems).HasConstraintName("FK_YourTable_Member");
        });

        modelBuilder.Entity<OrderTable>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__OrderTab__C3905BCF65A61B01");

            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.OrderFulfilled);
            entity.Property(e => e.OrderTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Member).WithMany(p => p.OrderTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderTabl__Membe__6A30C649");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Review__74BC79CEB80CE0CA");

            entity.HasOne(d => d.Game).WithMany(p => p.Reviews).HasConstraintName("FK__Review__GameId__628FA481");

            entity.HasOne(d => d.Member).WithMany(p => p.Reviews).HasConstraintName("FK__Review__MemberId__6477ECF3");

            entity.HasOne(d => d.Status).WithMany(p => p.Reviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__StatusId__6383C8BA");
        });

        modelBuilder.Entity<ReviewStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__ReviewSt__C8EE2063E3615C6E");
        });

        modelBuilder.Entity<WishList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WishList__3213E83F74B360B4");

            entity.HasOne(d => d.Game).WithMany(p => p.WishLists).HasConstraintName("FK__WishList__GameId__09A971A2");

            entity.HasOne(d => d.Member).WithMany(p => p.WishLists).HasConstraintName("FK__WishList__Member__08B54D69");
        });

        modelBuilder.Entity<GameRating>(entity =>
        {
            entity.ToTable("GameRating");
            entity.HasKey(e => e.Id).HasName("PK__GameRati__3213E83FAADD8C02");

            entity.HasOne(d => d.Game).WithMany(p => p.GameRatings).HasConstraintName("FK__GameRatin__GameI__2BFE89A6");

            entity.HasOne(d => d.Member).WithMany(p => p.GameRatings).HasConstraintName("FK__GameRatin__Membe__2B0A656D");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cart__3213E83F608080A8");
            
            entity.Property(e => e.IsPhysical).HasDefaultValue(0);

            entity.HasOne(d => d.Game).WithMany(p => p.Carts).HasConstraintName("FK__Cart__GameId__236943A5");

            entity.HasOne(d => d.Member).WithMany(p => p.Carts).HasConstraintName("FK__Cart__MemberId__22751F6C");
        });

        modelBuilder.Entity<OwnedGame>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OwnedGam__3213E83FD8565454");

            entity.HasOne(d => d.Game).WithMany(p => p.OwnedGames).HasConstraintName("FK__OwnedGame__GameI__2739D489");

            entity.HasOne(d => d.Member).WithMany(p => p.OwnedGames).HasConstraintName("FK__OwnedGame__Membe__2645B050");
        });

        modelBuilder.Entity<GameAverageRating>().HasNoKey().ToView("GameAverageRating");

        modelBuilder.Entity<GameRatingReport>().HasNoKey().ToView("GameRatingReport");
        modelBuilder.Entity<MemberListReport>().HasNoKey().ToView("MemberListReport");
        modelBuilder.Entity<SalesReport>().HasNoKey().ToView("SalesReport");
        modelBuilder.Entity<WishListReport>().HasNoKey().ToView("WishListReport");
        modelBuilder.Entity<EventsRegistrationsReport>().HasNoKey().ToView("EventsRegistrationsReport"); 
        modelBuilder.Entity<MemberOrderDetailsReport>().HasNoKey().ToView("MemberOrderDetailsReport"); 


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
