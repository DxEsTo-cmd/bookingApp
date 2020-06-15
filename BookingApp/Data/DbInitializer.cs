using BookingApp.Data.Models;
using BookingApp.Helpers;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Data
{
    public class DbInitializer
    {
        readonly ApplicationDbContext context;
        readonly UserManager<ApplicationUser> userManager;
        readonly RoleManager<IdentityRole> roleManager;

        ApplicationUser superAdmin;

        public DbInitializer(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        /// <summary>
        /// Initializes persistent storage with at least single SuperAdmin (at production). 
        /// For debugging, adds dummy users and data.
        /// <para>Enable context.Database.EnsureDeleted() to force full reseeding with each run.</para>
        /// </summary>
        public async Task Initialize(bool UseStoreProc = true)
        {
#if DEBUG
            // WARNING! Wipes the entire database.
            //context.Database.EnsureDeleted();
#endif
            //make sure DB is created
            bool isDbFresh = context.Database.EnsureCreated();

            await EnsureCompetentSuperAdmin();
#if DEBUG
            if (isDbFresh)
            {
                if(UseStoreProc)
                    await StoreProcFuncRepository.LoadAllToDb(context);
                await SeedDummyData();

            }
#else
            if(UseStoreProc)
            {
                await StoreProcFuncRepository.DeleteAllFromDb(context);
                await StoreProcFuncRepository.LoadAllToDb(context);
            }
#endif
        }

        /// <summary>
        /// Creating/Granting competency for the SuperAdmin.
        /// </summary>
        /// <returns></returns>
        async Task EnsureCompetentSuperAdmin()
        {
            //make sure we have basic roles
            if (!await roleManager.RoleExistsAsync(RoleTypes.Admin))
                await roleManager.CreateAsync(new IdentityRole(RoleTypes.Admin));
            if (!await roleManager.RoleExistsAsync(RoleTypes.User))
                await roleManager.CreateAsync(new IdentityRole(RoleTypes.User));

            //making sure we have competent SuperAdmin
            var superAdminEmail = "superadmin@admin.cow";  //temporary: replace with the real SuperAdmin email address

            superAdmin = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdmin == null)//create new if no luck
            {
                superAdmin = new ApplicationUser() { UserName = "SuperAdmin", Email = superAdminEmail };
                await userManager.CreateAsync(user: superAdmin, password: superAdmin.UserName);//temporary: discard password once SuperAdmin has real email address
            }

            //ensuring SuperAdmin is entitled to all roles
            if (!await userManager.IsInRoleAsync(superAdmin, RoleTypes.Admin))
                await userManager.AddToRoleAsync(superAdmin, RoleTypes.Admin);

            if (!await userManager.IsInRoleAsync(superAdmin, RoleTypes.User))
                await userManager.AddToRoleAsync(superAdmin, RoleTypes.User);

            //unblocking and approving SuperAdmin
            superAdmin.ApprovalStatus = true;
            superAdmin.IsBlocked = false;
            await userManager.UpdateAsync(superAdmin);
        }


        async Task SeedDummyData()
        {
            var dummyUsernames = new[] {
                "Tiger",// 0
                "Elephant",// 1
                "Lion",// 2
                "Bear",// 3
                "Cheetah",// 4
                "Wolf",// 5
                "Camel",// 6
                "Eagle",// 7
                "Mantis"// 8
            };

#region Dummy data source
            var rand = new Random();
            const string loremIpsum = "The core of the city consists predominately of wall-to-wall buildings, with blocks of clustered low-rises made out of a variety of old and new buildings. Under Combine rule, certain residential buildings in the city are used as accommodations for citizens. Conditions in such housings are typically seen as poor, with very few luxuries and constant inspection and raids by Civil Protection. However, some city infrastructure, such as power plants, are maintained by the Combine, and electricity is made widely available from both traditional sources and Combine generators. The Combine themselves occupied some former government buildings, such as the Overwatch Nexus, to help keep control over the city. The city was large enough to provide all necessary needs for the citizens before the Combines occupation. This is supported by the presence of a hospital, several cafés and restaurants, office buildings, and underground city systems; most of which are still intact but abandoned. The outskirts of City 17 features industrial districts and additional Soviet - style housing, most of which are considered off - limits to citizens.The industrial districts are seen linked to the city via railway lines and canals. As there was little emphasis in maintaining non - essential parts of the city, many areas of City 17 suffered from urban decay prior to the Citadel's explosion.";
#endregion

#region Users
            var users = new Dictionary<string, ApplicationUser> { { superAdmin.UserName, superAdmin } };

            for (int i = 0; i < dummyUsernames.Length; i++)
            {
                string name = dummyUsernames[i];
                string password = name;// password == name
                bool isAdmin = i < 2;// first two dummies are admins

                bool? approvalStatus = null; // basically, all are newcomers

                if (i < 6) // approved all up to Wolf
                    approvalStatus = true;

                if (i == 3 || i == 7) // rejects: Bear & Eagle
                    approvalStatus = false;

                bool isBlocked = (i == 1 || i == 3 || i == 5);// block Elphant, Bear, Wolf

                string email = $"{name}@{(isAdmin ? RoleTypes.Admin : RoleTypes.User)}.cow".ToLower();//e.g. lion@admin.cow & camel@user.cow

                var user = new ApplicationUser() { UserName = name, Email = email };
                await userManager.CreateAsync(user, password);

                await userManager.AddToRoleAsync(user, RoleTypes.User);
                if (isAdmin)
                    await userManager.AddToRoleAsync(user, RoleTypes.Admin);

                user.IsBlocked = isBlocked;
                user.ApprovalStatus = approvalStatus;
                await userManager.UpdateAsync(user);

                users.Add(user.UserName, user);
            }
#endregion

#region Rules
            var rules = new Dictionary<string, Rule> {
                { "Defaultest",      new Rule() { Title = "Стандартний",    MinTime = 1,  MaxTime = 1440, } },
                { "Rooms",           new Rule() { Title = "Кімната",         MinTime = 60, MaxTime = 480, StepTime = 30, ServiceTime = 0,   ReuseTimeout = 0,   PreOrderTimeLimit = 1440 } },
                { "Teslas",          new Rule() { Title = "Тесла",        MinTime = 60, MaxTime = 300, StepTime = 30, ServiceTime = 180, ReuseTimeout = 0,   PreOrderTimeLimit = 360 } },
                { "BudMaterial",         new Rule() { Title = "Буд матеріали",       MinTime = 1,  MaxTime = 15,  StepTime = 1,  ServiceTime = 0,   ReuseTimeout = 240, PreOrderTimeLimit = 120 } },
                { "Bikes",    new Rule() { Title = "Велосипед",  MinTime = 30, MaxTime = 80,  StepTime = 10, ServiceTime = 40,  ReuseTimeout = 300, PreOrderTimeLimit = 1200 } },
                { "Inactive Rule",   new Rule() { Title = "Неактивне правило", MinTime = 30, MaxTime = 80,  StepTime = 10, ServiceTime = 40,  ReuseTimeout = 300, PreOrderTimeLimit = 1200, IsActive = false } }
            };
            rules.ToList().ForEach(e => e.Value.Creator = e.Value.Updater = superAdmin);

            //pushing into EF
            context.Rules.AddRange(rules.Select(e => e.Value));
            #endregion

            #region Images

            var images = new List<Image>
            {
                new Image { ImagePath = "/assets/resources/Resources/city-bike-1.jpg", IsPrimary = true},
                new Image { ImagePath = "/assets/resources/Resources/city-bike-2.jpg", IsPrimary = false}
            };

            images.ToList().ForEach(e => e.Creator = e.Updater = superAdmin);

            context.Images.AddRange(images);

            #endregion

            #region Folders
            var Folders = new Dictionary<string, Folder> {
                { "Town Hall", new Folder() { Title = "Оренда товарів Івано-Франківськ" } },
                { "Bike Rental", new Folder() { Title = "Оренда велосипедів", Image = @"/assets/resources/Folders/bike-folder.jpg" } }
            };
            Folders.Add("Spire Balcony", new Folder() { Title = "Електроніка", ParentFolder = Folders["Town Hall"], DefaultRule = rules["Inactive Rule"] });
            Folders.Add("Gala Balcony", new Folder() { Title = "Автомобілі", ParentFolder = Folders["Town Hall"] });
            Folders.Add("Museums", new Folder() { Title = "Кімната", ParentFolder = Folders["Town Hall"], IsActive = false });

            Folders.ToList().ForEach(e => e.Value.Creator = e.Value.Updater = superAdmin);

            context.Folders.AddRange(Folders.Select(e => e.Value));
#endregion

#region Resources
            var resources = new Dictionary<int, Resource> {
                {  1, new Resource() { Title = "Оренда ігрової приставки PS4",          Folder = Folders["Spire Balcony"], Rule = rules["Defaultest"], Price = 100, Image = new List<Image> { new Image { ImagePath = "/assets/resources/Resources/PS4.jpg", IsPrimary = true } } } },
                {  2, new Resource() { Title = "Оренда телефона IPhone XS",         Folder = Folders["Spire Balcony"], Rule = rules["BudMaterial"], Price = 120 } },
                {  3, new Resource() { Title = "Оренда телевізора Samsung UX3567",                  Folder = Folders["Spire Balcony"], Rule = rules["Bikes"], Price = 330 } },

                {  4, new Resource() { Title = "Оренда автомобіля Tesla Model X",      Folder = Folders["Gala Balcony"],  Rule = rules["Teslas"], Price = 140 } },

                {  5, new Resource() { Title = "Оренда бензопили",   Folder = Folders["Town Hall"],     Rule = rules["BudMaterial"], Price = 220, Image = new List<Image> { new Image { ImagePath = "/assets/resources/Resources/lumbarjack.jpg", IsPrimary = true } } } },

                {  6, new Resource() { Title = "Кімната в центрі міста",        Folder = Folders["Museums"],       Rule = rules["Rooms"], IsActive = false, Price = 130 } },
                {  7, new Resource() { Title = "Кімната у Крихівцях",            Folder = Folders["Museums"],       Rule = rules["Rooms"], IsActive = false, Price = 340 } },
                {  8, new Resource() { Title = "Кімната у будинку біля ріки",        Folder = Folders["Museums"],       Rule = rules["Rooms"] } },

                {  9, new Resource() { Title = "Інший ресурс",                                            Rule = rules["Bikes"], Price = 140} },

                { 10, new Resource() { Title = "Оренда спортивних велосипедів #2000", Folder = Folders["Bike Rental"],   Rule = rules["Inactive Rule"], Price = 270, Image =  new List<Image> { images[0] , images[1] } } },
                { 11, new Resource() { Title = "Велосипеди для міста(BikeRent.ua)",   Folder = Folders["Bike Rental"],   Rule = rules["Bikes"], Price = 310, Image =  new List<Image> { images[0] , images[1] } } },
                { 12, new Resource() { Title = "Шосейні велосипеди", Folder = Folders["Bike Rental"],   Rule = rules["Bikes"], Price = 130, Image =  new List<Image> { images[0] , images[1] } } },
                { 13, new Resource() { Title = "Спортивний велосипед",   Folder = Folders["Bike Rental"],   Rule = rules["Bikes"], IsActive = false, Price = 130, Image =  new List<Image> { images[0] , images[1] } } },
                { 14, new Resource() { Title = "Оренда шуруповерта",   Folder = Folders["Town Hall"],     Rule = rules["BudMaterial"], Price = 130, Image = new List<Image> { new Image { ImagePath = "/assets/resources/Resources/shurupovert.jpeg", IsPrimary = true } } } },
                { 15, new Resource() { Title = "Оренда бетономішалки",   Folder = Folders["Town Hall"],   Rule = rules["BudMaterial"], Price = 130, Image = new List<Image> { new Image { ImagePath = "/assets/resources/Resources/beton.jpg", IsPrimary = true } } } },

            };

            //fill authorship & description (random stuff)
            resources.ToList().ForEach(e =>
            {
                e.Value.Creator = e.Value.Updater = superAdmin;
                switch (e.Value.Title)
                {
                    case "Оренда ігрової приставки PS4":
                    case "Оренда телефона IPhone XS":
                    case "Оренда телевізора Samsung UX3567":
                        e.Value.Description = "Оренда електроніки, додатковий опис для користувача";
                        break;
                    case "Оренда автомобіля Tesla Model X":
                        e.Value.Description = "Автомобіль Tesla Model X чорного коліру, додаткова інформація для користувача";
                        break;
                    case "Оренда бензопили":
                        e.Value.Description = "Додаткова інформація";
                        break;
                    case "Кімната в центрі міста": 
                    case "Кімната у Крихівцях":
                    case "Кімната у будинку біля ріки":
                        e.Value.Description = "Додаткова інформація для користувача при оренді кімнати";
                        break;
                    case "Інший ресурс":
                        e.Value.Description = "Додаткова інформація для користувача";
                        break;
                    case "Оренда спортивних велосипедів #2000":
                    case "Велосипеди для міста(BikeRent.ua)":
                    case "Шосейні велосипеди":
                    case "Спортивний велосипед":
                        e.Value.Description = "Додаткова інформація для користувача";
                        break;
                    default:
                        break;
                }
            });

            //pushing into EF
            context.Resources.AddRange(resources.Select(e => e.Value));
#endregion

            //saving changes to DB before seeding booking, so the rules have default storage values.
            context.SaveChanges();

            //SeedBookingsSimple(rand, loremIpsum, users, resources);
            SeedBookingsNonOverlapping(rand, loremIpsum, users, resources);

            //saving changes to DB
            context.SaveChanges();
        }

        /// <summary>
        /// Seeds bookings without overlaps. For each resource, fill bookings from the end.
        /// </summary>
        private void SeedBookingsNonOverlapping(Random rand, string loremIpsum, Dictionary<string, ApplicationUser> users, Dictionary<int, Resource> resources)
        {
            const int day = 60 * 24;
            const int historyWriteMinutesLimit = -day * 365;
            const double terminationChance = 0.2;

            foreach (var entry in resources)
            {
                Resource r = entry.Value;

            #region Per-resource variables
                int stepMinutes = r.Rule.StepTime ?? 1;
                int preOrderLimit = r.Rule.PreOrderTimeLimit < 1 ? day : (r.Rule.PreOrderTimeLimit ?? 0);

                int maxMinutes =  r.Rule.MaxTime ?? 1440;
                int maxSteps = maxMinutes / stepMinutes;

                int minMinutes = r.Rule.MinTime ?? 1;
                int minSteps = minMinutes / stepMinutes;

                int serviceMinutes = r.Rule.ServiceTime ?? 0;

                int postUsageDelayMinutesLimit = rand.Next(maxMinutes, day);
            #endregion

                // The value used in a loop, for correct time determination
                int earliestOccupiedMinute = preOrderLimit + maxMinutes;

                var perResourceBookings = new List<Booking>();

                while(earliestOccupiedMinute > historyWriteMinutesLimit)
                {
                    int busyPlanMinutes = rand.Next(minSteps, maxSteps + 1) * stepMinutes;
                    int postUsageMinutesDelay = rand.Next(postUsageDelayMinutesLimit);

                    int plannedEndMinutes = earliestOccupiedMinute - serviceMinutes - postUsageMinutesDelay;
                    int? factualEndMinutes = plannedEndMinutes;

                    int plannedStartMinutes = plannedEndMinutes - busyPlanMinutes;
                    int? factualStartMinutes = plannedStartMinutes;

                    bool terminationHappened = rand.Next((int)(1/terminationChance)) == 0;
                    int? terminatedAtMinutes = null;

                    if (terminationHappened)
                    {
                        terminatedAtMinutes = plannedEndMinutes - rand.Next(preOrderLimit / 2);

                        bool isPreCancelled = terminatedAtMinutes <= plannedStartMinutes;

                        if (isPreCancelled)
                            factualStartMinutes = factualEndMinutes = null;
                        else
                            factualEndMinutes = terminatedAtMinutes; 
                    }

                    earliestOccupiedMinute = factualStartMinutes ?? earliestOccupiedMinute;

                    DateTime now = DateTime.Now;
                    var creator = users.OrderBy(e => rand.Next()).First().Value;
                    var startTime = now + TimeSpan.FromMinutes(plannedStartMinutes);
                    var terminationTime = terminatedAtMinutes == null ? (DateTime?)null : now + TimeSpan.FromMinutes((int)terminatedAtMinutes);
                    var createdTime = startTime - TimeSpan.FromMinutes(preOrderLimit * rand.Next(100) / 100);
                    var updatedTime = terminationTime ?? createdTime;

                    DateTime startTimeBooking = startTime;
                    DateTime endTimeBooking = now + TimeSpan.FromMinutes(plannedEndMinutes);

                    perResourceBookings.Add(
                        new Booking
                        {
                            Note = "Додаткова інформація користувача про бронювання",
                            Resource = r,
                            Price = r.Price * (endTimeBooking - startTimeBooking).Minutes,
                            StartTime = startTime,
                            EndTime = now + TimeSpan.FromMinutes(plannedEndMinutes),
                            TerminationTime = terminationTime,
                            Creator = creator,
                            Updater = new[] { creator, users.First().Value }[rand.Next(2)],
                            CreatedTime = createdTime,
                            UpdatedTime = updatedTime
                        }
                    );
                }

                //pushing into EF
                perResourceBookings.Reverse();
                context.Bookings.AddRange(perResourceBookings);
            }
        }
    }
}