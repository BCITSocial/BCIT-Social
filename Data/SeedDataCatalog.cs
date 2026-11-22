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
            "BITMAN student, esports organizer, and spreadsheet enthusiast who enjoys bringing technical and business teams together.",
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
            "CST student focused on web APIs, open-source software, and helping beginners feel welcome.")
    ];

    internal static CommunitySeed[] Communities { get; } =
    [
        new(
            "developers",
            "maya",
            "BCIT Developers Network",
            "Club, Web Development, Burnaby Campus",
            "A welcoming community for BCIT students who build software. Share projects, find teammates, ask technical questions, and learn from students across programs.",
            "communities/community-developers.jpg",
            ["daniel", "lucas", "emma", "noah", "ava", "james", "evelyn", "henry"],
            ["daniel"]),

        new(
            "cst-study",
            "noah",
            "CST Study Hub",
            "Program & Intake, Study Group, Burnaby Campus",
            "A student-run study space for CST coursework, exam preparation, peer support, and sharing useful learning resources.",
            "communities/community-cst-study.jpg",
            ["maya", "emma", "lucas", "james", "daniel", "henry"],
            ["james"]),

        new(
            "design",
            "ava",
            "Downtown Design Collective",
            "Program & Intake, UI/UX Design, Downtown Campus",
            "A collaborative space for UI, UX, web, and new media students to exchange critiques, share portfolios, and practice design skills.",
            "communities/community-design.jpg",
            ["charlotte", "harper", "isabella", "sophia", "evelyn", "maya"],
            ["charlotte"]),

        new(
            "cybersecurity",
            "liam",
            "BCIT Cybersecurity Club",
            "Academic, Cybersecurity, Downtown Campus",
            "Beginner-friendly security discussions, capture-the-flag practice, secure coding resources, and responsible cybersecurity learning.",
            "communities/community-cybersecurity.jpg",
            ["sophia", "evelyn", "mason", "henry", "daniel", "maya"],
            ["evelyn"]),

        new(
            "ai-data",
            "emma",
            "BCIT AI & Data Society",
            "Club, AI & Machine Learning, Burnaby Campus",
            "A practical community for machine learning, data science, responsible AI, and student project collaboration.",
            "communities/community-ai-data.jpg",
            ["maya", "noah", "henry", "james", "sophia", "lucas", "daniel"],
            ["henry"]),

        new(
            "photography",
            "harper",
            "BCIT Photography Club",
            "Arts & Culture, Photography, Burnaby Campus",
            "Photo walks, editing sessions, constructive critique, and opportunities to document student life across BCIT campuses.",
            "communities/community-photography.jpg",
            ["oliver", "ava", "charlotte", "amelia", "isabella", "mia"],
            ["charlotte"]),

        new(
            "hiking",
            "oliver",
            "Weekend Hiking Crew",
            "Sports & Recreation, Hiking, Burnaby Campus",
            "Student-organized hikes around Metro Vancouver. Routes, transit plans, safety details, and difficulty levels are posted before every trip.",
            "communities/community-hiking.jpg",
            ["maya", "sophia", "mia", "mason", "amelia", "lucas", "ben"],
            ["sophia"]),

        new(
            "esports",
            "ethan",
            "BCIT Esports",
            "Interest & Hobbies, Esports, Burnaby Campus",
            "Casual games, competitive teams, campus tournaments, and a friendly place to find other students to play with.",
            "communities/community-esports.jpg",
            ["mason", "james", "daniel", "liam", "evelyn", "lucas"],
            ["mason"]),

        new(
            "career",
            "isabella",
            "Career and Co-op Prep",
            "Career & Co-op, Resumes & Interviews, Burnaby Campus",
            "Resume feedback, interview practice, portfolio reviews, networking tips, and peer support for internships and co-op applications.",
            null,
            ["maya", "ava", "noah", "emma", "sophia", "ethan", "amelia", "charlotte", "ben"],
            ["amelia"]),

        new(
            "volunteers",
            "lucas",
            "BCIT Student Volunteers",
            "Social, Volunteering, Burnaby Campus",
            "Find volunteer opportunities, organize student-led service projects, and meet people while supporting the wider community.",
            null,
            ["maya", "amelia", "mia", "isabella", "oliver", "daniel", "harper"],
            ["amelia"]),

        new(
            "basketball",
            "mason",
            "BCIT Intramural Basketball",
            "Sports & Recreation, Basketball, Burnaby Campus",
            "Pickup games, team formation, practice sessions, and intramural basketball updates for students of all experience levels.",
            null,
            ["mia", "ethan", "james", "noah", "henry", "ben"],
            ["mia"]),

        new(
            "aerospace",
            "ben",
            "Aerospace Student Network",
            "Program & Intake, Networking, Aerospace Campus",
            "A community for Aerospace Campus students to share program advice, industry opportunities, and technical resources.",
            null,
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
        new(
            "dev-hackathon",
            "maya",
            "developers",
            "Looking for two teammates for the weekend hackathon",
            "I am putting together a small team for the upcoming student hackathon. The idea is a campus accessibility tool that helps students find elevators, quiet spaces, and accessible entrances. We could use one more developer and someone interested in UI/UX.",
            8,
            ["coding", "projects", "events"],
            ["posts/post-hackathon-team.jpg"]),

        new(
            "dev-git",
            "daniel",
            "developers",
            "Git workflow guide for student teams",
            "I wrote a short guide covering feature branches, pull requests, code reviews, and resolving common merge conflicts. It is aimed at teams starting their first shared project. Let me know which sections need more examples.",
            31,
            ["coding", "resources", "web-development"],
            ["posts/post-git-workflow.jpg"]),

        new(
            "cst-midterm",
            "noah",
            "cst-study",
            "Midterm review board is ready",
            "The review board in SE12 now has practice questions for databases, algorithms, and web development. Add your own questions, but please include an answer or a useful reference when possible.",
            13,
            ["study", "resources", "help"],
            ["posts/post-study-board.jpg"]),

        new(
            "cst-quiet-room",
            "james",
            "cst-study",
            "Quiet study room booked Thursday evening",
            "I booked a group room in the library from 5:30 to 8:00 on Thursday. We will start with the database practice set and finish with questions from anyone who joins.",
            51,
            ["study", "events"],
            ["posts/post-study-room.jpg"]),

        new(
            "design-portfolio",
            "ava",
            "design",
            "Portfolio critique exchange",
            "Post one project you are comfortable sharing and describe the kind of feedback you want. I am especially interested in case-study structure and explaining design decisions clearly.",
            10,
            ["design", "career"],
            ["posts/post-portfolio-review.jpg"]),

        new(
            "design-figma",
            "charlotte",
            "design",
            "Free Figma component library for student projects",
            "I cleaned up the buttons, inputs, cards, alerts, and navigation components from our term project. The components use variables and include basic accessibility notes.",
            69,
            ["design", "resources", "projects"],
            ["posts/post-figma-library.jpg"]),

        new(
            "cyber-ctf",
            "liam",
            "cybersecurity",
            "Beginner CTF practice this Friday",
            "We are running a guided capture-the-flag session with introductory web, Linux, and cryptography challenges. No previous CTF experience is required. Bring a laptop and arrive a few minutes early for setup.",
            16,
            ["cybersecurity", "events", "workshops"],
            ["posts/post-ctf-session.jpg"]),

        new(
            "cyber-passwords",
            "evelyn",
            "cybersecurity",
            "Password manager setup resources",
            "Here are the resources from our discussion about password managers, recovery codes, and multi-factor authentication. Please do not post personal account details or screenshots containing recovery information.",
            83,
            ["cybersecurity", "resources", "technology"],
            ["posts/post-password-manager.jpg"]),

        new(
            "ai-study-group",
            "emma",
            "ai-data",
            "Machine learning reading group topics",
            "For the next three meetings, I propose model evaluation, data leakage, and responsible use of generative AI. Reply with papers, videos, or tutorials that explain these topics clearly.",
            21,
            ["artificial-intelligence", "study", "resources"],
            ["posts/post-ml-reading-group.jpg"]),

        new(
            "ai-showcase",
            "henry",
            "ai-data",
            "Showcase your small AI or data project",
            "The next project night is meant for unfinished work too. A notebook, visualization, tiny model, or thoughtful experiment is enough. The goal is to share what you learned rather than present a polished product.",
            47,
            ["artificial-intelligence", "projects", "events"],
            ["posts/post-ai-showcase.jpg"]),

        new(
            "photo-walk",
            "harper",
            "photography",
            "Deer Lake golden-hour photo walk",
            "We will meet near the main entrance and walk the lakeside trail before sunset. Phones and cameras are equally welcome. The forecast may change, so check the event page before leaving.",
            18,
            ["photography", "events"],
            ["posts/post-photo-walk.jpg"]),

        new(
            "photo-editing",
            "charlotte",
            "photography",
            "Before-and-after editing discussion",
            "Share an original image and your edited version if you are comfortable. Include a short note about what you changed and what feeling you wanted the final photo to have.",
            92,
            ["photography", "design"],
            ["posts/post-photo-editing.jpg"]),

        new(
            "hiking-transit",
            "oliver",
            "hiking",
            "Transit plan for the Lynn Canyon hike",
            "We will take the SeaBus and connect to the bus as a group. The route is moderate and may be muddy, so bring proper footwear, water, and a rain layer.",
            25,
            ["hiking", "events"],
            ["posts/post-hiking-transit.jpg"]),

        new(
            "hiking-safety",
            "sophia",
            "hiking",
            "A quick reminder about winter trail safety",
            "Short daylight hours change how we plan weekend hikes. Please bring a light, warm layer, water, and the required transit fare. We will turn around as a group if conditions are poor.",
            118,
            ["hiking", "resources"],
            ["posts/post-trail-safety.jpg"]),

        new(
            "esports-rocket-league",
            "ethan",
            "esports",
            "Rocket League tournament registration",
            "Registration is open for teams of three. Solo players can also register and we will help form balanced teams. The tournament is casual, but standard sportsmanship rules still apply.",
            27,
            ["esports", "events"],
            ["posts/post-rocket-league.jpg"]),

        new(
            "esports-casual",
            "mason",
            "esports",
            "Casual game night suggestions",
            "What should we include at the next casual game night? Local multiplayer games are preferred so people can rotate in without waiting too long.",
            75,
            ["esports", "help"],
            ["posts/post-game-night.jpg"]),

        new(
            "career-resume",
            "isabella",
            "career",
            "Resume checklist for co-op applications",
            "Before submitting, verify that your dates are consistent, links work, accomplishments are specific, and the most relevant experience appears near the top. Export to PDF and check it on another device.",
            12,
            ["career", "resources"],
            ["posts/post-resume-checklist.jpg"]),

        new(
            "career-interviews",
            "amelia",
            "career",
            "Mock interview partners",
            "Use this thread to find a mock interview partner. Include your program, the role type you are preparing for, and times you are available this week.",
            39,
            ["career", "networking", "help"],
            ["posts/post-mock-interviews.jpg"]),

        new(
            "volunteer-food-bank",
            "lucas",
            "volunteers",
            "Food bank volunteer shift needs four more people",
            "The Saturday sorting shift still has four spaces available. The work involves standing and lifting light boxes. Full instructions and the meeting location are on the event page.",
            19,
            ["volunteering", "events"],
            ["posts/post-food-bank.jpg"]),

        new(
            "volunteer-ideas",
            "amelia",
            "volunteers",
            "Ideas for a campus service project",
            "We are collecting ideas for a small project that can be completed before the end of term. Suggestions should be realistic for a student team and useful to the campus or local community.",
            64,
            ["volunteering", "projects"],
            ["posts/post-service-project.jpg"]),

        new(
            "basketball-pickup",
            "mason",
            "basketball",
            "Wednesday pickup game",
            "We have the gym booked Wednesday evening. Please RSVP so we can decide whether to run full-court games or smaller rotations. Beginners are welcome.",
            22,
            ["basketball", "events"],
            []),

        new(
            "basketball-team",
            "mia",
            "basketball",
            "Looking for one more intramural player",
            "Our team needs one more player for the upcoming intramural schedule. We are recreational and mainly looking for someone reliable who can attend most games.",
            55,
            ["basketball", "help"],
            []),

        new(
            "aerospace-networking",
            "ben",
            "aerospace",
            "Questions for the industry networking panel",
            "Post questions you would like the panel moderator to ask. Topics can include apprenticeships, workplace expectations, certifications, and entering the industry after graduation.",
            15,
            ["career", "networking", "events"],
            []),

        new(
            "aerospace-study",
            "henry",
            "aerospace",
            "Electrical systems study session",
            "A few of us are reviewing electrical system diagrams after class on Tuesday. Bring one topic you want to explain and one question you want the group to work through.",
            72,
            ["study", "technology"],
            [])
    ];

    internal static CommentSeed[] Comments { get; } =
    [
        new("c01", "dev-hackathon", "ava", "I am interested in helping with the interface and the accessibility research.", 1),
        new("c02", "dev-hackathon", "lucas", "I can help with the API and location data. I will message you my availability.", 2),
        new("c03", "dev-git", "james", "A section on recovering from an accidental commit would be really useful.", 3),
        new("c04", "cst-midterm", "emma", "I added two database normalization questions and linked the relevant notes.", 1),
        new("c05", "cst-midterm", "daniel", "Could we add a short section for common LINQ mistakes too?", 2),
        new("c06", "cst-quiet-room", "maya", "I will be there around 5:45 after my lab.", 4),
        new("c07", "design-portfolio", "charlotte", "I would love feedback on whether my case study has enough process detail.", 1),
        new("c08", "design-figma", "harper", "The accessibility notes make this much more useful than most free component sets.", 5),
        new("c09", "cyber-ctf", "evelyn", "Will the setup work on Windows, or should we install a virtual machine first?", 1),
        new("c10", "cyber-passwords", "sophia", "The recovery-code reminder is important. It is easy to forget until a phone is lost.", 6),
        new("c11", "ai-study-group", "henry", "I have a good introductory article about data leakage that includes practical examples.", 2),
        new("c12", "ai-showcase", "maya", "I can show the recommendation prototype from our last sprint, even though it is still rough.", 4),
        new("c13", "photo-walk", "oliver", "I will bring a small tripod if anyone wants to try longer exposures near sunset.", 1),
        new("c14", "photo-editing", "ava", "It would be interesting to compare subtle edits with more stylized versions.", 4),
        new("c15", "hiking-transit", "maya", "Thanks for including transit details. I should be able to join from Waterfront.", 2),
        new("c16", "hiking-safety", "mia", "Microspikes might also be worth mentioning if the temperature drops.", 7),
        new("c17", "esports-rocket-league", "james", "I registered solo. Happy to play with other casual players.", 2),
        new("c18", "career-resume", "noah", "Checking the PDF on a phone caught a formatting problem in mine last week.", 1),
        new("c19", "career-interviews", "ben", "I am preparing for technical apprenticeship interviews and am free Thursday afternoon.", 3),
        new("c20", "volunteer-food-bank", "amelia", "I signed up for the morning shift and can meet everyone near the bus stop.", 2),
        new("c21", "basketball-pickup", "mia", "I can bring an extra ball and arrive early to help set up.", 1),
        new("c22", "aerospace-networking", "isabella", "A question about making the most of the first ninety days would be helpful.", 2),

        new("r01", "dev-hackathon", "maya", "That would be perfect. I will share the rough user flow tonight.", 2, "c01"),
        new("r02", "dev-git", "daniel", "Good suggestion. I will add reset, revert, and reflog examples.", 5, "c03"),
        new("r03", "cst-midterm", "noah", "Yes, I will create a LINQ section before tomorrow evening.", 4, "c05"),
        new("r04", "cyber-ctf", "liam", "Windows is fine. We will provide a browser-based option and setup help.", 3, "c09"),
        new("r05", "ai-study-group", "emma", "Please add it to the shared resource list when you get a chance.", 5, "c11"),
        new("r06", "photo-walk", "harper", "Great idea. We should have enough time near the lake before it gets dark.", 3, "c13"),
        new("r07", "career-interviews", "amelia", "There are two other aerospace students in the signup sheet, so I will connect you.", 6, "c19"),
        new("r08", "basketball-pickup", "mason", "Thanks! I will be there about twenty minutes before the booking starts.", 3, "c21")
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
        new("charlotte", "evelyn", "Pending", 2)
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
        new("charlotte", "design-figma", 3)
    ];

    internal static EventSeed[] Events { get; } =
    [
        new(
            "git-workshop",
            "maya",
            "developers",
            "Intro to Git and GitHub Workshop",
            4,
            18,
            120,
            "Burnaby Campus",
            "SE12-101",
            "A hands-on introduction to repositories, branches, commits, pull requests, and resolving simple merge conflicts. Bring a laptop with Git installed.",
            ["maya", "noah", "ava", "lucas", "james", "daniel"],
            ["emma", "evelyn"]),

        new(
            "cst-study-jam",
            "noah",
            "cst-study",
            "CST Midterm Study Jam",
            7,
            17,
            180,
            "Burnaby Campus",
            "SW1-1205",
            "A structured review session covering databases, web development, and algorithms. We will work through practice questions in small groups.",
            ["noah", "maya", "emma", "james", "daniel", "henry"],
            ["lucas"]),

        new(
            "portfolio-night",
            "ava",
            "design",
            "Portfolio Review Night",
            10,
            18,
            150,
            "Downtown Campus",
            "Tech Collider",
            "Bring one portfolio project for friendly, structured feedback from other design and development students.",
            ["ava", "charlotte", "harper", "isabella"],
            ["maya", "sophia", "evelyn"]),

        new(
            "ctf-session",
            "liam",
            "cybersecurity",
            "Beginner Capture-the-Flag Session",
            6,
            17,
            150,
            "Downtown Campus",
            "Room 825",
            "Guided beginner challenges covering web security, Linux fundamentals, and introductory cryptography. No previous CTF experience is required.",
            ["liam", "evelyn", "sophia", "mason", "daniel"],
            ["maya", "henry"]),

        new(
            "ai-showcase-event",
            "emma",
            "ai-data",
            "Student AI and Data Project Showcase",
            13,
            18,
            120,
            "Burnaby Campus",
            "SE6-233",
            "An informal evening for students to demonstrate small AI, data science, and visualization projects and discuss what they learned.",
            ["emma", "henry", "maya", "noah", "james", "lucas"],
            ["sophia", "daniel"]),

        new(
            "photo-walk-event",
            "harper",
            "photography",
            "Deer Lake Golden-Hour Photo Walk",
            8,
            15,
            180,
            "Burnaby Campus",
            "Meet outside the BCIT Library",
            "We will travel to Deer Lake together for a relaxed photo walk. Phones and cameras are welcome. Bring weather-appropriate clothing.",
            ["harper", "oliver", "charlotte", "ava", "amelia"],
            ["mia", "isabella"]),

        new(
            "grouse-hike",
            "oliver",
            "hiking",
            "Grouse Mountain Weekend Hike",
            17,
            8,
            360,
            "Burnaby Campus",
            "Meet at Production Way-University Station",
            "A challenging weekend hike for students comfortable with sustained elevation. Final routing depends on weather and trail conditions.",
            ["oliver", "sophia", "maya", "mason", "ben"],
            ["lucas", "amelia"]),

        new(
            "rocket-league",
            "ethan",
            "esports",
            "BCIT Rocket League Tournament",
            11,
            18,
            210,
            "Burnaby Campus",
            "SE2 Great Hall",
            "A casual three-versus-three tournament. Register as a team or join the solo-player list and we will help form teams.",
            ["ethan", "mason", "james", "daniel", "liam"],
            ["evelyn", "lucas"]),

        new(
            "resume-dropin",
            "isabella",
            "career",
            "Resume Review Drop-In",
            5,
            12,
            180,
            "Burnaby Campus",
            "Library Collaboration Room 3",
            "Bring a printed or digital resume for short peer-review sessions. This is a student-led event and not an official BCIT advising appointment.",
            ["isabella", "amelia", "maya", "noah", "ava"],
            ["emma", "ben", "charlotte"]),

        new(
            "food-bank",
            "lucas",
            "volunteers",
            "Community Food Bank Volunteer Shift",
            9,
            9,
            240,
            "Burnaby Campus",
            "Meet at the Canada Way bus stop",
            "Volunteers will travel together to help sort donations and prepare food hampers. Closed-toe shoes are required.",
            ["lucas", "amelia", "maya", "mia", "harper"],
            ["oliver", "daniel"]),

        new(
            "basketball-night",
            "mason",
            "basketball",
            "Burnaby Campus Pickup Basketball",
            3,
            19,
            120,
            "Burnaby Campus",
            "Recreation Services Gym",
            "A recreational pickup session open to all skill levels. RSVP so the organizers can plan teams and rotations.",
            ["mason", "mia", "ethan", "james", "noah", "ben"],
            ["henry"]),

        new(
            "aerospace-panel",
            "ben",
            "aerospace",
            "Aerospace Industry Networking Evening",
            15,
            18,
            150,
            "Aerospace Campus",
            "Main Hangar Classroom",
            "A student networking evening featuring an industry panel and an open question period about apprenticeships and early-career opportunities.",
            ["ben", "henry", "oliver", "isabella"],
            ["ethan", "sophia"]),

        new(
            "past-design",
            "ava",
            "design",
            "Introduction to Accessible Interface Design",
            -12,
            18,
            120,
            "Downtown Campus",
            "Room 830",
            "A completed beginner workshop covering semantic structure, keyboard navigation, colour contrast, and clear form feedback.",
            ["ava", "charlotte", "maya", "evelyn"],
            ["harper"]),

        new(
            "past-social",
            "lucas",
            "volunteers",
            "Campus Cleanup and Coffee",
            -24,
            10,
            150,
            "Burnaby Campus",
            "SE2 Courtyard",
            "A completed student volunteer event that combined a short campus cleanup with coffee and introductions afterward.",
            ["lucas", "maya", "amelia", "mia", "daniel"],
            ["harper", "oliver"])
    ];

    internal static IEnumerable<string> ImageFiles =>
        Users
            .Where(user => user.PhotoFile is not null)
            .Select(user => user.PhotoFile!)
            .Concat(
                Communities
                    .Where(community => community.BannerFile is not null)
                    .Select(community => community.BannerFile!))
            .Concat(Posts.SelectMany(post => post.PhotoFiles))
            .Distinct(StringComparer.OrdinalIgnoreCase);
}