namespace BCIT_Social.Data;

/// <summary>
/// Deterministic, realistic sample content for BCIT Social.
/// </summary>
internal static class SeedDataCatalog
{
    internal const string DEMO_PASSWORD = "Demo123!";

    internal sealed record UserSeed(
        string Key,
        string FirstName,
        string LastName,
        string Email,
        string Program,
        string Campus,
        string Bio,
        string? PhotoFile = null);

    internal sealed record CommunitySeed(
        string Key,
        string OwnerKey,
        string Name,
        string Type,
        string Description,
        string? BannerFile,
        string[] MemberKeys,
        string[] ModeratorKeys);

    internal sealed record PostSeed(
        string Key,
        string AuthorKey,
        string CommunityKey,
        string Title,
        string Content,
        int HoursAgo,
        string[] Tags,
        string[] PhotoFiles);

    internal sealed record CommentSeed(
        string Key,
        string PostKey,
        string AuthorKey,
        string Content,
        int HoursAfterPost,
        string? ParentKey = null);

    internal sealed record FriendshipSeed(
        string RequesterKey,
        string RecipientKey,
        string Status,
        int DaysAgo);

    internal sealed record BookmarkSeed(
        string UserKey,
        string PostKey,
        int DaysAgo);

    internal sealed record EventSeed(
        string Key,
        string CreatorKey,
        string CommunityKey,
        string Title,
        int DaysFromToday,
        int StartHour,
        int DurationMinutes,
        string Campus,
        string Location,
        string Description,
        string[] GoingUserKeys,
        string[] InterestedUserKeys);

    internal static UserSeed[] Users { get; } =
    [
        new(
            "maya",
            "Maya",
            "Chen",
            "demo.maya.chen@my.bcit.ca",
            "Computer Systems Technology",
            "Burnaby Campus",
            "CST student interested in full-stack development, accessibility, and student-led technology projects.",
            "profiles/profile-maya.jpg"),

        new(
            "noah",
            "Noah",
            "Patel",
            "seed.noah.patel@my.bcit.ca",
            "Computer Systems Technology",
            "Burnaby Campus",
            "Second-year CST student who enjoys backend development, databases, and helping classmates prepare for exams.",
            "profiles/profile-noah.jpg"),

        new(
            "ava",
            "Ava",
            "Williams",
            "seed.ava.williams@my.bcit.ca",
            "Digital Design and Development",
            "Downtown Campus",
            "Designer and frontend developer interested in inclusive interfaces, typography, and thoughtful user experiences.",
            "profiles/profile-ava.jpg"),

        new(
            "liam",
            "Liam",
            "Garcia",
            "seed.liam.garcia@my.bcit.ca",
            "Cybersecurity",
            "Downtown Campus",
            "Cybersecurity student learning defensive security, threat analysis, and secure application development.",
            "profiles/profile-liam.jpg"),

        new(
            "emma",
            "Emma",
            "Johnson",
            "seed.emma.johnson@my.bcit.ca",
            "Computer Systems Technology",
            "Burnaby Campus",
            "CST student exploring machine learning, data visualization, and practical applications of AI.",
            "profiles/profile-emma.jpg"),

        new(
            "oliver",
            "Oliver",
            "Kim",
            "seed.oliver.kim@my.bcit.ca",
            "Mechanical Engineering",
            "Burnaby Campus",
            "Mechanical engineering student who spends weekends hiking, taking photos, and building small robotics projects.",
            "profiles/profile-oliver.jpg"),

        new(
            "sophia",
            "Sophia",
            "Brown",
            "seed.sophia.brown@my.bcit.ca",
            "Computer Information Technology",
            "Downtown Campus",
            "CIT student interested in cloud infrastructure, networking, and connecting new students with campus resources.",
            "profiles/profile-sophia.jpg"),

        new(
            "ethan",
            "Ethan",
            "Lee",
            "seed.ethan.lee@my.bcit.ca",
            "Business Information Technology Management",
            "Burnaby Campus",
            "BITMAN student and esports organizer who enjoys bringing technical and business teams together.",
            "profiles/profile-ethan.jpg"),

        new(
            "isabella",
            "Isabella",
            "Martin",
            "seed.isabella.martin@my.bcit.ca",
            "Marketing Management",
            "Burnaby Campus",
            "Marketing student focused on professional development, student networking, and building a strong portfolio.",
            "profiles/profile-isabella.jpg"),

        new(
            "lucas",
            "Lucas",
            "Thompson",
            "seed.lucas.thompson@my.bcit.ca",
            "Computer Systems Technology",
            "Burnaby Campus",
            "Software developer and student volunteer who likes open-source projects and community events.",
            "profiles/profile-lucas.jpg"),

        new(
            "mia",
            "Mia",
            "Anderson",
            "seed.mia.anderson@my.bcit.ca",
            "Accounting",
            "Burnaby Campus",
            "Accounting student balancing coursework with intramural sports and student club activities.",
            "profiles/profile-mia.jpg"),

        new(
            "mason",
            "Mason",
            "Taylor",
            "seed.mason.taylor@my.bcit.ca",
            "Computer Information Technology",
            "Downtown Campus",
            "CIT student interested in systems administration, basketball, and practical automation.",
            "profiles/profile-mason.jpg"),

        new(
            "amelia",
            "Amelia",
            "Moore",
            "seed.amelia.moore@my.bcit.ca",
            "Marketing Management",
            "Burnaby Campus",
            "Marketing student interested in event promotion, social media strategy, and volunteer work.",
            "profiles/profile-amelia.jpg"),

        new(
            "james",
            "James",
            "Jackson",
            "seed.james.jackson@my.bcit.ca",
            "Computer Systems Technology",
            "Burnaby Campus",
            "CST student who enjoys algorithms, peer tutoring, and late-night debugging sessions.",
            "profiles/profile-james.jpg"),

        new(
            "harper",
            "Harper",
            "White",
            "seed.harper.white@my.bcit.ca",
            "Graphic Communications Technology",
            "Burnaby Campus",
            "Visual storyteller interested in photography, print design, and documenting student life.",
            "profiles/profile-harper.jpg"),

        new(
            "ben",
            "Benjamin",
            "Harris",
            "seed.benjamin.harris@my.bcit.ca",
            "Aircraft Maintenance Engineer",
            "Aerospace Campus",
            "Aerospace student interested in aircraft systems, industry networking, and hands-on technical work."),

        new(
            "evelyn",
            "Evelyn",
            "Clark",
            "seed.evelyn.clark@my.bcit.ca",
            "Computer Information Technology",
            "Downtown Campus",
            "CIT student exploring mobile development, cloud platforms, and cybersecurity."),

        new(
            "henry",
            "Henry",
            "Lewis",
            "seed.henry.lewis@my.bcit.ca",
            "Electrical and Computer Engineering",
            "Burnaby Campus",
            "Engineering student interested in embedded systems, robotics, and collaborative technical projects."),

        new(
            "charlotte",
            "Charlotte",
            "Walker",
            "seed.charlotte.walker@my.bcit.ca",
            "New Media Design and Web Development",
            "Downtown Campus",
            "New media student creating interactive experiences through design, code, motion, and photography."),

        new(
            "daniel",
            "Daniel",
            "Hall",
            "seed.daniel.hall@my.bcit.ca",
            "Computer Systems Technology",
            "Burnaby Campus",
            "CST student focused on web APIs, open-source software, and helping beginners feel welcome."),

        new(
            "priya",
            "Priya",
            "Shah",
            "seed.priya.shah@my.bcit.ca",
            "Software Systems Developer",
            "Downtown Campus",
            "Software development student interested in cloud systems, accessible mobile experiences, and collaborative projects.",
            "profiles/profile-priya.jpg"),

        new(
            "jordan",
            "Jordan",
            "Nguyen",
            "seed.jordan.nguyen@my.bcit.ca",
            "Civil Engineering",
            "Burnaby Campus",
            "Civil engineering student interested in sustainable infrastructure, open data, basketball, and community projects.",
            "profiles/profile-jordan.jpg"),

        new(
            "samira",
            "Samira",
            "Ahmed",
            "seed.samira.ahmed@my.bcit.ca",
            "Nursing",
            "Burnaby Campus",
            "Nursing student focused on clinical learning, student wellness, volunteering, and supporting classmates.",
            "profiles/profile-samira.jpg"),

        new(
            "alex",
            "Alex",
            "Romero",
            "seed.alex.romero@my.bcit.ca",
            "Broadcast and Online Journalism",
            "Burnaby Campus",
            "Journalism student interested in podcast production, photography, campus stories, and digital media.",
            "profiles/profile-alex.jpg"),

        new(
            "chloe",
            "Chloe",
            "Park",
            "seed.chloe.park@my.bcit.ca",
            "Environmental Engineering Technology",
            "Burnaby Campus",
            "Environmental engineering student interested in sustainability, public transit, volunteering, and data-driven projects.",
            "profiles/profile-chloe.jpg")
    ];

    internal static CommunitySeed[] Communities { get; } =
    [
        new(
            "developers",
            "maya",
            "BCIT Developers Network",
            "Club, Web Development, Burnaby Campus",
            "A welcoming community for BCIT students who build software, share projects, find teammates, and ask technical questions.",
            "communities/community-developers.jpg",
            ["daniel", "lucas", "emma", "noah", "ava", "james", "evelyn", "henry", "priya", "jordan"],
            ["daniel"]),

        new(
            "cst-study",
            "noah",
            "CST Study Hub",
            "Program & Intake, Study Group, Burnaby Campus",
            "A student-run study space for CST coursework, exam preparation, peer support, and learning resources.",
            "communities/community-cst-study.jpg",
            ["maya", "emma", "lucas", "james", "daniel", "henry"],
            ["james"]),

        new(
            "design",
            "ava",
            "Downtown Design Collective",
            "Program & Intake, UI/UX Design, Downtown Campus",
            "A collaborative space for UI, UX, web, and new media students to exchange critiques and share portfolios.",
            "communities/community-design.jpg",
            ["charlotte", "harper", "isabella", "sophia", "evelyn", "maya", "alex", "priya"],
            ["charlotte"]),

        new(
            "cybersecurity",
            "liam",
            "BCIT Cybersecurity Club",
            "Academic, Cybersecurity, Downtown Campus",
            "Beginner-friendly security discussions, capture-the-flag practice, secure coding resources, and responsible learning.",
            "communities/community-cybersecurity.jpg",
            ["sophia", "evelyn", "mason", "henry", "daniel", "maya", "priya"],
            ["evelyn"]),

        new(
            "ai-data",
            "emma",
            "BCIT AI & Data Society",
            "Club, AI & Machine Learning, Burnaby Campus",
            "A practical community for machine learning, data science, responsible AI, and student project collaboration.",
            "communities/community-ai-data.jpg",
            ["maya", "noah", "henry", "james", "sophia", "lucas", "daniel", "chloe"],
            ["henry"]),

        new(
            "photography",
            "harper",
            "BCIT Photography Club",
            "Arts & Culture, Photography, Burnaby Campus",
            "Photo walks, editing sessions, constructive critique, and opportunities to document student life.",
            "communities/community-photography.jpg",
            ["oliver", "ava", "charlotte", "amelia", "isabella", "mia", "alex"],
            ["charlotte"]),

        new(
            "hiking",
            "oliver",
            "Weekend Hiking Crew",
            "Sports & Recreation, Hiking, Burnaby Campus",
            "Student-organized hikes around Metro Vancouver with routes, transit plans, and safety details.",
            "communities/community-hiking.jpg",
            ["maya", "sophia", "mia", "mason", "amelia", "lucas", "ben", "jordan", "samira", "chloe"],
            ["sophia"]),

        new(
            "esports",
            "ethan",
            "BCIT Esports",
            "Interest & Hobbies, Esports, Burnaby Campus",
            "Casual games, competitive teams, campus tournaments, and a friendly place to find other players.",
            "communities/community-esports.jpg",
            ["mason", "james", "daniel", "liam", "evelyn", "lucas"],
            ["mason"]),

        new(
            "career",
            "isabella",
            "Career and Co-op Prep",
            "Career & Co-op, Resumes & Interviews, Burnaby Campus",
            "Resume feedback, interview practice, portfolio reviews, networking tips, and peer support.",
            "communities/community-career.jpg",
            ["maya", "ava", "noah", "emma", "sophia", "ethan", "amelia", "charlotte", "ben", "priya", "jordan", "samira", "alex", "chloe"],
            ["amelia"]),

        new(
            "volunteers",
            "lucas",
            "BCIT Student Volunteers",
            "Social, Volunteering, Burnaby Campus",
            "Find volunteer opportunities, organize student-led service projects, and support the wider community.",
            "communities/community-volunteers.jpg",
            ["maya", "amelia", "mia", "isabella", "oliver", "daniel", "harper", "jordan", "samira", "chloe"],
            ["amelia"]),

        new(
            "basketball",
            "mason",
            "BCIT Intramural Basketball",
            "Sports & Recreation, Basketball, Burnaby Campus",
            "Pickup games, team formation, practice sessions, and intramural updates for all experience levels.",
            "communities/community-basketball.jpg",
            ["mia", "ethan", "james", "noah", "henry", "ben", "jordan"],
            ["mia"]),

        new(
            "aerospace",
            "ben",
            "Aerospace Student Network",
            "Program & Intake, Networking, Aerospace Campus",
            "A community for Aerospace Campus students to share program advice and industry opportunities.",
            "communities/community-aerospace.jpg",
            ["henry", "oliver", "isabella", "ethan", "sophia"],
            ["henry"])
    ];

    internal static string[] Tags { get; } =
    [
        "announcements",
        "artificial-intelligence",
        "basketball",
        "career",
        "coding",
        "cybersecurity",
        "design",
        "esports",
        "events",
        "help",
        "hiking",
        "networking",
        "photography",
        "projects",
        "resources",
        "study",
        "technology",
        "volunteering",
        "web-development",
        "workshops"
    ];

    internal static PostSeed[] Posts { get; } =
    [
        new("dev-hackathon", "maya", "developers",
            "Looking for two teammates for the weekend hackathon",
            "I am putting together a team for a campus accessibility tool that helps students find elevators, quiet spaces, and accessible entrances.",
            8, ["coding", "projects", "events"], ["posts/post-hackathon-team.jpg"]),

        new("dev-git", "daniel", "developers",
            "Git workflow guide for student teams",
            "I wrote a guide covering feature branches, pull requests, code reviews, and resolving common merge conflicts.",
            31, ["coding", "resources", "web-development"], ["posts/post-git-workflow.jpg"]),

        new("cst-midterm", "noah", "cst-study",
            "Midterm review board is ready",
            "The review board now has practice questions for databases, algorithms, and web development.",
            13, ["study", "resources", "help"], ["posts/post-study-board.jpg"]),

        new("cst-quiet-room", "james", "cst-study",
            "Quiet study room booked Thursday evening",
            "I booked a library group room from 5:30 to 8:00 for database practice and open questions.",
            51, ["study", "events"], ["posts/post-study-room.jpg"]),

        new("design-portfolio", "ava", "design",
            "Portfolio critique exchange",
            "Post one project and describe the kind of feedback you want, especially around case-study structure.",
            10, ["design", "career"], ["posts/post-portfolio-review.jpg"]),

        new("design-figma", "charlotte", "design",
            "Free Figma component library for student projects",
            "I cleaned up reusable buttons, inputs, cards, alerts, and navigation components with accessibility notes.",
            69, ["design", "resources", "projects"], ["posts/post-figma-library.jpg"]),

        new("cyber-ctf", "liam", "cybersecurity",
            "Beginner CTF practice this Friday",
            "We are running guided introductory web, Linux, and cryptography challenges. No previous experience is required.",
            16, ["cybersecurity", "events", "workshops"], ["posts/post-ctf-session.jpg"]),

        new("cyber-passwords", "evelyn", "cybersecurity",
            "Password manager setup resources",
            "Here are resources about password managers, recovery codes, and multi-factor authentication.",
            83, ["cybersecurity", "resources", "technology"], ["posts/post-password-manager.jpg"]),

        new("ai-study-group", "emma", "ai-data",
            "Machine learning reading group topics",
            "The next meetings cover model evaluation, data leakage, and responsible use of generative AI.",
            21, ["artificial-intelligence", "study", "resources"], ["posts/post-ml-reading-group.jpg"]),

        new("ai-showcase", "henry", "ai-data",
            "Showcase your small AI or data project",
            "A notebook, visualization, tiny model, or thoughtful experiment is enough. The project does not need to be polished.",
            47, ["artificial-intelligence", "projects", "events"], ["posts/post-ai-showcase.jpg"]),

        new("photo-walk", "harper", "photography",
            "Deer Lake golden-hour photo walk",
            "We will meet near the main entrance and walk the lakeside trail before sunset.",
            18, ["photography", "events"], ["posts/post-photo-walk.jpg"]),

        new("photo-editing", "charlotte", "photography",
            "Before-and-after editing discussion",
            "Share an original image, its edited version, and a short note about what you changed.",
            92, ["photography", "design"], ["posts/post-photo-editing.jpg"]),

        new("hiking-transit", "oliver", "hiking",
            "Transit plan for the Lynn Canyon hike",
            "We will take the SeaBus and connect to the bus as a group. Bring proper footwear and a rain layer.",
            25, ["hiking", "events"], ["posts/post-hiking-transit.jpg"]),

        new("hiking-safety", "sophia", "hiking",
            "A quick reminder about winter trail safety",
            "Bring a light, warm layer, water, and required transit fare. We will turn around if conditions are poor.",
            118, ["hiking", "resources"], ["posts/post-trail-safety.jpg"]),

        new("esports-rocket-league", "ethan", "esports",
            "Rocket League tournament registration",
            "Registration is open for teams of three. Solo players can register and be placed on a team.",
            27, ["esports", "events"], ["posts/post-rocket-league.jpg"]),

        new("esports-casual", "mason", "esports",
            "Casual game night suggestions",
            "What should we include at the next casual game night? Local multiplayer games are preferred.",
            75, ["esports", "help"], ["posts/post-game-night.jpg"]),

        new("career-resume", "isabella", "career",
            "Resume checklist for co-op applications",
            "Verify dates, links, accomplishments, and formatting before exporting your resume to PDF.",
            12, ["career", "resources"], ["posts/post-resume-checklist.jpg"]),

        new("career-interviews", "amelia", "career",
            "Mock interview partners",
            "Use this thread to find a mock interview partner. Include your program and availability.",
            39, ["career", "networking", "help"], ["posts/post-mock-interviews.jpg"]),

        new("volunteer-food-bank", "lucas", "volunteers",
            "Food bank volunteer shift needs four more people",
            "The Saturday sorting shift still has four spaces available. Instructions are on the event page.",
            19, ["volunteering", "events"], ["posts/post-food-bank.jpg"]),

        new("volunteer-ideas", "amelia", "volunteers",
            "Ideas for a campus service project",
            "We are collecting realistic project ideas that can be completed before the end of term.",
            64, ["volunteering", "projects"], ["posts/post-service-project.jpg"]),

        new("basketball-pickup", "mason", "basketball",
            "Wednesday pickup game",
            "The gym is booked Wednesday evening. RSVP so we can plan teams and rotations.",
            22, ["basketball", "events"], ["posts/post-basketball-pickup.jpg"]),

        new("basketball-team", "mia", "basketball",
            "Looking for one more intramural player",
            "Our recreational team needs one reliable player who can attend most games.",
            55, ["basketball", "help"], ["posts/post-basketball-team.jpg"]),

        new("aerospace-networking", "ben", "aerospace",
            "Questions for the industry networking panel",
            "Post questions about apprenticeships, workplace expectations, certifications, and entering the industry.",
            15, ["career", "networking", "events"], ["posts/post-aerospace-networking.jpg"]),

        new("aerospace-study", "henry", "aerospace",
            "Electrical systems study session",
            "We are reviewing electrical system diagrams after class on Tuesday.",
            72, ["study", "technology"], ["posts/post-aerospace-study.jpg"]),

        new("cloud-lab-notes", "priya", "developers",
            "Cloud deployment lab notes and troubleshooting guide",
            "I organized notes about environment variables, connection strings, deployment logs, and common failures.",
            6, ["technology", "coding", "resources"], ["posts/post-cloud-lab-notes.jpg"]),

        new("clinical-placement", "samira", "career",
            "Clinical placement preparation checklist",
            "A checklist covering documents, commute planning, meals, appropriate clothing, and first-shift questions.",
            14, ["career", "resources", "help"], ["posts/post-clinical-placement.jpg"]),

        new("sustainable-campus", "chloe", "volunteers",
            "Ideas for a more sustainable campus",
            "I am collecting realistic ideas for reducing waste and improving sustainable transportation.",
            20, ["volunteering", "projects", "technology"], ["posts/post-sustainable-campus.jpg"]),

        new("podcast-workshop", "alex", "design",
            "Beginner podcast production resources",
            "Here are microphone, editing, interview, and publishing resources from our introductory session.",
            29, ["design", "resources", "technology"], ["posts/post-podcast-workshop.jpg"]),

        new("civil-open-data", "jordan", "developers",
            "Using municipal open data in student projects",
            "Municipalities publish transportation, construction, and geographic datasets useful for student projects.",
            37, ["projects", "technology", "resources"], ["posts/post-civil-open-data.jpg"]),

        new("mobile-accessibility", "priya", "design",
            "Mobile accessibility testing checklist",
            "This checklist covers text scaling, touch targets, screen-reader labels, contrast, and reduced motion.",
            45, ["design", "web-development", "resources"], ["posts/post-mobile-accessibility.jpg"]),

        new("coop-headshots", "alex", "photography",
            "Free student headshot afternoon",
            "Photography students are offering simple headshots for resumes and professional profiles.",
            53, ["photography", "career", "events"], ["posts/post-coop-headshots.jpg"]),

        new("volunteer-orientation", "samira", "volunteers",
            "What to expect at volunteer orientation",
            "Orientation covers safety, scheduling, communication, accessibility, and cancelling a shift.",
            62, ["volunteering", "resources"], ["posts/post-volunteer-orientation.jpg"]),

        new("basketball-skills", "jordan", "basketball",
            "Beginner basketball skills session",
            "Before pickup, we are covering passing, positioning, shooting form, and basic defensive movement.",
            71, ["basketball", "events", "help"], ["posts/post-basketball-skills.jpg"]),

        new("aircraft-inspection", "ben", "aerospace",
            "Aircraft inspection study diagram",
            "I created a labelled study diagram for the inspection sequence discussed this week.",
            80, ["study", "technology", "resources"], ["posts/post-aircraft-inspection.jpg"]),

        new("open-source-first-issue", "daniel", "developers",
            "How to choose a first open-source issue",
            "Look for contribution instructions, recent maintainer activity, a working setup, and beginner issues.",
            94, ["coding", "projects", "resources"], ["posts/post-open-source-first-issue.jpg"]),

        new("campus-food-guide", "chloe", "volunteers",
            "Affordable food options around Burnaby Campus",
            "I started a list of affordable meals, dietary options, microwaves, water stations, and grocery stores.",
            108, ["resources", "help"], ["posts/post-campus-food-guide.jpg"])
    ];

    internal static CommentSeed[] Comments { get; } =
    [
        new("c01", "dev-hackathon", "ava", "I am interested in helping with the interface and accessibility research.", 1),
        new("c02", "dev-hackathon", "lucas", "I can help with the API and location data.", 2),
        new("c03", "dev-git", "james", "A section on recovering from an accidental commit would be useful.", 3),
        new("c04", "cst-midterm", "emma", "I added two database normalization questions.", 1),
        new("c05", "cst-midterm", "daniel", "Could we add common LINQ mistakes too?", 2),
        new("c06", "cst-quiet-room", "maya", "I will be there around 5:45 after my lab.", 4),
        new("c07", "design-portfolio", "charlotte", "I would love feedback on my case-study detail.", 1),
        new("c08", "design-figma", "harper", "The accessibility notes make this especially useful.", 5),
        new("c09", "cyber-ctf", "evelyn", "Will the setup work on Windows?", 1),
        new("c10", "cyber-passwords", "sophia", "The recovery-code reminder is important.", 6),
        new("c11", "ai-study-group", "henry", "I have an introductory article about data leakage.", 2),
        new("c12", "ai-showcase", "maya", "I can show our recommendation prototype.", 4),
        new("c13", "photo-walk", "oliver", "I will bring a small tripod.", 1),
        new("c14", "photo-editing", "ava", "Comparing subtle and stylized edits would be interesting.", 4),
        new("c15", "hiking-transit", "maya", "Thanks for including transit details.", 2),
        new("c16", "hiking-safety", "mia", "Microspikes may be worth mentioning.", 7),
        new("c17", "esports-rocket-league", "james", "I registered solo.", 2),
        new("c18", "career-resume", "noah", "Checking the PDF on a phone caught a problem in mine.", 1),
        new("c19", "career-interviews", "ben", "I am preparing for apprenticeship interviews.", 3),
        new("c20", "volunteer-food-bank", "amelia", "I signed up for the morning shift.", 2),
        new("c21", "basketball-pickup", "mia", "I can bring an extra ball.", 1),
        new("c22", "aerospace-networking", "isabella", "A question about the first ninety days would help.", 2),

        new("c23", "cloud-lab-notes", "maya", "The deployment-log section would have saved me time.", 1),
        new("c24", "clinical-placement", "amelia", "The commute-planning reminder is useful.", 2),
        new("c25", "sustainable-campus", "lucas", "A small waste audit could give us baseline numbers.", 3),
        new("c26", "podcast-workshop", "charlotte", "Could you add the noise-reduction settings?", 2),
        new("c27", "civil-open-data", "emma", "This could make a great visualization project.", 4),
        new("c28", "mobile-accessibility", "ava", "Please add reduced-motion testing.", 3),
        new("c29", "coop-headshots", "isabella", "This will help students preparing applications.", 2),
        new("c30", "volunteer-orientation", "chloe", "Will there be a remote option?", 4),
        new("c31", "basketball-skills", "mia", "I can help demonstrate the passing drills.", 2),
        new("c32", "aircraft-inspection", "henry", "The sequence is much clearer in this format.", 5),
        new("c33", "open-source-first-issue", "priya", "Checking maintainer activity first is good advice.", 3),
        new("c34", "campus-food-guide", "samira", "Adding opening hours would make this more useful.", 2),

        new("r01", "dev-hackathon", "maya", "I will share the rough user flow tonight.", 2, "c01"),
        new("r02", "dev-git", "daniel", "I will add reset, revert, and reflog examples.", 5, "c03"),
        new("r03", "cst-midterm", "noah", "I will create a LINQ section.", 4, "c05"),
        new("r04", "cyber-ctf", "liam", "Windows is fine; setup help will be available.", 3, "c09"),
        new("r05", "ai-study-group", "emma", "Please add it to the shared list.", 5, "c11"),
        new("r06", "photo-walk", "harper", "We should have enough time near the lake.", 3, "c13"),
        new("r07", "career-interviews", "amelia", "I will connect you with other aerospace students.", 6, "c19"),
        new("r08", "basketball-pickup", "mason", "I will arrive early to help set up.", 3, "c21"),
        new("r09", "sustainable-campus", "chloe", "I will bring a draft audit sheet.", 5, "c25"),
        new("r10", "podcast-workshop", "alex", "I will add screenshots and sample settings.", 4, "c26"),
        new("r11", "mobile-accessibility", "priya", "I will add that before sharing the final version.", 5, "c28"),
        new("r12", "volunteer-orientation", "samira", "We will post a video-call option.", 6, "c30")
    ];

    internal static FriendshipSeed[] Friendships { get; } =
    [
        new("maya", "noah", "Accepted", 120),
        new("maya", "ava", "Accepted", 103),
        new("maya", "emma", "Accepted", 91),
        new("maya", "lucas", "Accepted", 73),
        new("maya", "daniel", "Accepted", 62),
        new("maya", "sophia", "Accepted", 44),
        new("maya", "oliver", "Accepted", 31),
        new("noah", "james", "Accepted", 88),
        new("noah", "emma", "Accepted", 68),
        new("ava", "charlotte", "Accepted", 79),
        new("ava", "harper", "Accepted", 56),
        new("liam", "evelyn", "Accepted", 49),
        new("emma", "henry", "Accepted", 46),
        new("oliver", "sophia", "Accepted", 38),
        new("ethan", "mason", "Accepted", 34),
        new("isabella", "amelia", "Accepted", 29),
        new("lucas", "daniel", "Accepted", 24),
        new("mia", "mason", "Accepted", 19),
        new("ben", "henry", "Accepted", 17),
        new("harper", "maya", "Pending", 4),
        new("ben", "maya", "Pending", 2),
        new("maya", "isabella", "Pending", 3),
        new("maya", "ethan", "Pending", 1),
        new("charlotte", "evelyn", "Pending", 2),

        new("priya", "maya", "Accepted", 18),
        new("priya", "ava", "Accepted", 16),
        new("jordan", "mason", "Accepted", 15),
        new("samira", "amelia", "Accepted", 13),
        new("alex", "harper", "Accepted", 11),
        new("chloe", "lucas", "Accepted", 9),
        new("jordan", "maya", "Pending", 3),
        new("samira", "maya", "Pending", 2)
    ];

    internal static BookmarkSeed[] Bookmarks { get; } =
    [
        new("maya", "cst-midterm", 1),
        new("maya", "career-resume", 2),
        new("maya", "hiking-transit", 3),
        new("maya", "cyber-ctf", 4),
        new("noah", "dev-git", 2),
        new("noah", "ai-study-group", 3),
        new("ava", "career-resume", 1),
        new("ava", "photo-editing", 4),
        new("liam", "cyber-passwords", 2),
        new("emma", "ai-showcase", 1),
        new("oliver", "photo-walk", 2),
        new("sophia", "hiking-safety", 5),
        new("ethan", "esports-rocket-league", 1),
        new("isabella", "design-portfolio", 3),
        new("lucas", "volunteer-food-bank", 1),
        new("mia", "basketball-pickup", 2),
        new("ben", "aerospace-networking", 1),
        new("charlotte", "design-figma", 3),

        new("priya", "mobile-accessibility", 1),
        new("priya", "open-source-first-issue", 2),
        new("jordan", "basketball-skills", 1),
        new("jordan", "civil-open-data", 3),
        new("samira", "clinical-placement", 1),
        new("samira", "volunteer-orientation", 2),
        new("alex", "coop-headshots", 1),
        new("alex", "podcast-workshop", 2),
        new("chloe", "sustainable-campus", 1),
        new("chloe", "campus-food-guide", 2)
    ];

    internal static EventSeed[] Events { get; } =
    [
        new("git-workshop", "maya", "developers",
            "Intro to Git and GitHub Workshop",
            4, 18, 120, "Burnaby Campus", "SE12-101",
            "A hands-on introduction to repositories, branches, commits, pull requests, and merge conflicts.",
            ["maya", "noah", "ava", "lucas", "james", "daniel"],
            ["emma", "evelyn"]),

        new("cst-study-jam", "noah", "cst-study",
            "CST Midterm Study Jam",
            7, 17, 180, "Burnaby Campus", "SW1-1205",
            "A structured review session covering databases, web development, and algorithms.",
            ["noah", "maya", "emma", "james", "daniel", "henry"],
            ["lucas"]),

        new("portfolio-night", "ava", "design",
            "Portfolio Review Night",
            10, 18, 150, "Downtown Campus", "Tech Collider",
            "Bring one portfolio project for friendly, structured feedback.",
            ["ava", "charlotte", "harper", "isabella"],
            ["maya", "sophia", "evelyn"]),

        new("ctf-session", "liam", "cybersecurity",
            "Beginner Capture-the-Flag Session",
            6, 17, 150, "Downtown Campus", "Room 825",
            "Guided beginner challenges covering web security, Linux, and cryptography.",
            ["liam", "evelyn", "sophia", "mason", "daniel"],
            ["maya", "henry"]),

        new("ai-showcase-event", "emma", "ai-data",
            "Student AI and Data Project Showcase",
            13, 18, 120, "Burnaby Campus", "SE6-233",
            "An informal evening for students to demonstrate small AI and data projects.",
            ["emma", "henry", "maya", "noah", "james", "lucas"],
            ["sophia", "daniel"]),

        new("photo-walk-event", "harper", "photography",
            "Deer Lake Golden-Hour Photo Walk",
            8, 15, 180, "Burnaby Campus", "Meet outside the BCIT Library",
            "A relaxed photo walk. Phones and cameras are equally welcome.",
            ["harper", "oliver", "charlotte", "ava", "amelia"],
            ["mia", "isabella"]),

        new("grouse-hike", "oliver", "hiking",
            "Grouse Mountain Weekend Hike",
            17, 8, 360, "Burnaby Campus", "Production Way-University Station",
            "A challenging weekend hike dependent on weather and trail conditions.",
            ["oliver", "sophia", "maya", "mason", "ben"],
            ["lucas", "amelia"]),

        new("rocket-league", "ethan", "esports",
            "BCIT Rocket League Tournament",
            11, 18, 210, "Burnaby Campus", "SE2 Great Hall",
            "A casual three-versus-three tournament for teams and solo players.",
            ["ethan", "mason", "james", "daniel", "liam"],
            ["evelyn", "lucas"]),

        new("resume-dropin", "isabella", "career",
            "Resume Review Drop-In",
            5, 12, 180, "Burnaby Campus", "Library Collaboration Room 3",
            "Bring a printed or digital resume for short peer-review sessions.",
            ["isabella", "amelia", "maya", "noah", "ava"],
            ["emma", "ben", "charlotte"]),

        new("food-bank", "lucas", "volunteers",
            "Community Food Bank Volunteer Shift",
            9, 9, 240, "Burnaby Campus", "Canada Way bus stop",
            "Volunteers will help sort donations and prepare food hampers.",
            ["lucas", "amelia", "maya", "mia", "harper"],
            ["oliver", "daniel"]),

        new("basketball-night", "mason", "basketball",
            "Burnaby Campus Pickup Basketball",
            3, 19, 120, "Burnaby Campus", "Recreation Services Gym",
            "A recreational pickup session open to all skill levels.",
            ["mason", "mia", "ethan", "james", "noah", "ben"],
            ["henry"]),

        new("aerospace-panel", "ben", "aerospace",
            "Aerospace Industry Networking Evening",
            15, 18, 150, "Aerospace Campus", "Main Hangar Classroom",
            "An industry panel about apprenticeships and early-career opportunities.",
            ["ben", "henry", "oliver", "isabella"],
            ["ethan", "sophia"]),

        new("past-design", "ava", "design",
            "Introduction to Accessible Interface Design",
            -12, 18, 120, "Downtown Campus", "Room 830",
            "A completed workshop about semantic structure, keyboard navigation, and contrast.",
            ["ava", "charlotte", "maya", "evelyn"],
            ["harper"]),

        new("past-social", "lucas", "volunteers",
            "Campus Cleanup and Coffee",
            -24, 10, 150, "Burnaby Campus", "SE2 Courtyard",
            "A completed student volunteer cleanup and social event.",
            ["lucas", "maya", "amelia", "mia", "daniel"],
            ["harper", "oliver"]),

        new("cloud-lab", "maya", "developers",
            "Hands-On Cloud Deployment Lab",
            19, 18, 150, "Burnaby Campus", "SE12-102",
            "Deploy a small application while learning configuration, logs, and secrets.",
            ["maya", "priya", "daniel", "lucas", "emma"],
            ["noah", "evelyn"]),

        new("nursing-review", "isabella", "career",
            "Clinical Placement Preparation Evening",
            22, 17, 120, "Burnaby Campus", "SW3-1750",
            "A student-led session about communication, scheduling, transportation, and placement questions.",
            ["samira", "isabella", "amelia", "mia"],
            ["chloe", "maya"]),

        new("sustainability-fair", "lucas", "volunteers",
            "Student Sustainability Project Fair",
            26, 11, 240, "Burnaby Campus", "SE2 Great Hall",
            "Students will share sustainability ideas, projects, and environmental data.",
            ["lucas", "chloe", "jordan", "samira", "amelia"],
            ["maya", "oliver", "sophia"]),

        new("podcast-workshop-event", "ava", "design",
            "Beginner Podcast Production Workshop",
            16, 18, 150, "Downtown Campus", "Media Lab",
            "An introduction to interviews, microphones, recording, editing, and publishing.",
            ["ava", "alex", "charlotte", "harper"],
            ["isabella", "priya"]),

        new("civil-site-tour", "isabella", "career",
            "Civil Engineering Site Tour and Career Talk",
            30, 9, 300, "Burnaby Campus", "Meet outside SW1",
            "A guided site visit followed by a student career discussion.",
            ["jordan", "isabella", "ben", "henry"],
            ["chloe", "oliver"]),

        new("mobile-design-sprint", "ava", "design",
            "Accessible Mobile Design Sprint",
            24, 17, 180, "Downtown Campus", "Room 830",
            "Teams will redesign a mobile workflow with accessibility in mind.",
            ["ava", "priya", "charlotte", "alex", "maya"],
            ["evelyn", "emma"])
    ];

    // Event images are intentionally not included because CommunityEvent
    // does not currently store photo metadata.
    internal static IEnumerable<string> ImageFiles =>
        Users
            .Where(user => user.PhotoFile is not null)
            .Select(user => user.PhotoFile!)
            .Concat(
                Communities
                    .Where(community =>
                        community.BannerFile is not null)
                    .Select(community =>
                        community.BannerFile!))
            .Concat(
                Posts.SelectMany(
                    post => post.PhotoFiles))
            .Distinct(
                StringComparer.OrdinalIgnoreCase);
}