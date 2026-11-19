// ==============================================================================================
// ARCHITECTURAL CONCEPT: Curated Task Catalog Seed Data (Abu Dhabi Edition)
// Seeds 30 clinician-reviewed micro-habits across Movement, Sleep Routine, and Nutrition.
// Includes climate-adapted indoor walking options for extreme summer heat, gentle routines
// for desk and shift workers, and cultural wellness elements for Abu Dhabi residents.
// ==============================================================================================

using Gibo.Domain.Aggregates.PlanAggregate;
using Gibo.Domain.Aggregates.UserAggregate;

namespace Gibo.Infrastructure.Persistence.Seeding;

public static class TaskCatalogSeed
{
    public static readonly IReadOnlyList<TaskTemplate> CuratedTemplates = new List<TaskTemplate>
    {
        // ------------------------------------------------------------------------------------------
        // MOVEMENT (12 Templates)
        // ------------------------------------------------------------------------------------------
        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000001"), 1, GoalDomain.Movement, "Walk",
            MeasurementType.DurationMinutes, TaskDifficulty.Starter, 10, "mins", 10,
            EvidenceMode.Both, false, new[] { "LowImpact", "IndoorAC" }, "HabitFormation",
            "10-Minute Air-Conditioned Mall Walk",
            "مشي لمدة 10 دقائق داخل مركز تسوق مكيّف",
            "10 मिनट वातानुकूलित मॉल वॉक",
            "Perfect for avoiding daytime heat while maintaining active mobility.",
            "مثالي لتجنب حرارة النهار والحفاظ على النشاط البدني.",
            "दिन की गर्मी से बचते हुए सक्रिय रहने के लिए आदर्श।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000002"), 1, GoalDomain.Movement, "Stretch",
            MeasurementType.DurationMinutes, TaskDifficulty.Starter, 5, "mins", 5,
            EvidenceMode.Manual, false, new[] { "Seated", "LowImpact" }, "Prompting",
            "5-Minute Desk Mobility & Shoulder Relief",
            "5 دقائق لتمارين مرونة الأكتاف وتخفيف إجهاد المكتب",
            "5 मिनट डेस्क मोबिलिटी और कंधे का खिंचाव",
            "Relieves stiffness from extended desk and computer sessions.",
            "يخفف من التصلب والإجهاد الناتج عن الجلوس الطويل في العمل.",
            "लंबे समय तक डेस्क पर बैठने से होने वाली अकड़न को दूर करता है।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000003"), 1, GoalDomain.Movement, "Walk",
            MeasurementType.DurationMinutes, TaskDifficulty.Standard, 15, "mins", 15,
            EvidenceMode.Both, true, new[] { "EveningOnly", "CoolHours" }, "ActionPlanning",
            "15-Minute Evening Corniche Stroll",
            "نزهة مسائية لمدة 15 دقيقة على الكورنيش",
            "15 मिनट शाम को कॉर्निश पर टहलना",
            "Takes advantage of cooler evening coastal breezes along Abu Dhabi Corniche.",
            "استمتع بنسيم المساء المعتدل على كورنيش أبوظبي بعد غروب الشمس.",
            "शाम की ठंडी हवा में अबू धाबी कॉर्निश पर ताज़गी भरी सैर।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000004"), 1, GoalDomain.Movement, "Stand",
            MeasurementType.DurationMinutes, TaskDifficulty.Starter, 2, "mins", 2,
            EvidenceMode.Manual, false, new[] { "LowImpact", "OfficeSafe" }, "HabitStacking",
            "2-Minute Stand & Active Posture Reset",
            "دقيقتان للوقوف وتعديل وضعية الجسم",
            "2 मिनट खड़े होकर शरीर की मुद्रा सही करना",
            "Breaks continuous sedentary time to improve circulation.",
            "يكسر فترات الجلوس المتواصلة لتنشيط الدورة الدموية.",
            "रक्त संचार को बेहतर बनाने के लिए बैठने का ब्रेक लें।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000005"), 1, GoalDomain.Movement, "Climb",
            MeasurementType.Repetitions, TaskDifficulty.Standard, 2, "flights", 5,
            EvidenceMode.Both, false, new[] { "ModerateImpact" }, "ActionPlanning",
            "Stair Climber (2 Flight Walk-Up)",
            "صعود طابقين من الدرج بنشاط",
            "2 मंजिल सीढ़ियाँ चढ़ना",
            "Quick lower-body strength and cardiovascular activation.",
            "تنشيط سريع للقلب وتقوية عضلات الساقين.",
            "पैरों की मजबूती और हृदय गति के लिए एक तेज़ व्यायाम।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000006"), 1, GoalDomain.Movement, "Circuit",
            MeasurementType.DurationMinutes, TaskDifficulty.Standard, 7, "mins", 7,
            EvidenceMode.Manual, false, new[] { "LowImpact", "LivingRoom" }, "GradedTasks",
            "7-Minute Living Room Low-Impact Circuit",
            "تمرين منزلي خفيف لمدة 7 دقائق في غرفة المعيشة",
            "7 मिनट घर पर हल्का व्यायाम",
            "Gentle bodyweight moves suitable for any living space.",
            "حركات خفيفة بوزن الجسم تناسب أي مساحة منزلية.",
            "घर पर किया जाने वाला आसान और हल्का व्यायाम।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000007"), 1, GoalDomain.Movement, "Walk",
            MeasurementType.StepCount, TaskDifficulty.Starter, 1000, "steps", 10,
            EvidenceMode.Both, false, new[] { "IndoorPreferred" }, "SelfMonitoring",
            "1,000 Step Midday Errand Walk",
            "1,000 خطوة أثناء إنجاز مهام منتصف اليوم",
            "दोपहर के कामों के दौरान 1,000 कदम",
            "Accumulates daily baseline steps during indoor breaks or errands.",
            "تجميع خطوات النشاط اليومي أثناء فترات الاستراحة أو التسوق الداخلي.",
            "दैनिक कदमों का लक्ष्य आसानी से पूरा करने का तरीका।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000008"), 1, GoalDomain.Movement, "March",
            MeasurementType.DurationMinutes, TaskDifficulty.Standard, 10, "mins", 10,
            EvidenceMode.Both, false, new[] { "IndoorAC" }, "HabitFormation",
            "10-Minute Morning Indoor March",
            "10 دقائق مشي في المكان صباحاً داخل المنزل",
            "सुबह 10 मिनट घर के अंदर मार्च",
            "Wakes up major muscle groups before leaving for work.",
            "إيقاظ العضلات الرئيسية ومستويات الطاقة قبل بدء يوم العمل.",
            "काम पर जाने से पहले शरीर को ऊर्जावान बनाएं।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000009"), 1, GoalDomain.Movement, "Stretch",
            MeasurementType.DurationMinutes, TaskDifficulty.Starter, 5, "mins", 5,
            EvidenceMode.Manual, false, new[] { "LowImpact", "JointFriendly" }, "Restructuring",
            "5-Minute Hamstring & Lower Back Release",
            "5 دقائق لإطالة أوتار الركبة وأسفل الظهر",
            "5 मिनट पीठ और पैरों का खिंचाव",
            "Reduces lower spine tension from car commutes and office chairs.",
            "يخفف الضغط على أسفل الظهر الناتج عن قيادة السيارة والجلوس المكتبي.",
            "गाड़ी चलाने और कुर्सी पर बैठने से पीठ का तनाव कम करें।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000010"), 1, GoalDomain.Movement, "Walk",
            MeasurementType.DurationMinutes, TaskDifficulty.Standard, 15, "mins", 15,
            EvidenceMode.Both, true, new[] { "CoolHours", "Park" }, "ActionPlanning",
            "15-Minute Shaded Park Walk (Early Morning)",
            "مشي لمدة 15 دقيقة في حديقة مظللة في الصباح الباكر",
            "सुबह-सुबह किसी पार्क में 15 मिनट की सैर",
            "Natural morning light helps calibrate your circadian rhythm.",
            "التعرض لضوء الصباح الطبيعي يعزز انتظام ساعتك البيولوجية.",
            "सुबह की धूप आपकी जैविक घड़ी को संतुलित करने में मदद करती है।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000011"), 1, GoalDomain.Movement, "Yoga",
            MeasurementType.DurationMinutes, TaskDifficulty.Starter, 8, "mins", 8,
            EvidenceMode.Manual, false, new[] { "Seated", "OfficeSafe" }, "GradedTasks",
            "8-Minute Seated Chair Mobility",
            "8 دقائق تمارين مرونة واستطالة على الكرسي",
            "कुर्सी पर बैठकर 8 मिनट का आसान स्ट्रेचिंग",
            "Ideal for tired days when standing feels like too much effort.",
            "مثالي للأيام المجهدة عندما يكون الوقوف متعباً.",
            "थकान भरे दिनों के लिए आसान और आरामदायक स्ट्रेचिंग।"),

        TaskTemplate.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000012"), 1, GoalDomain.Movement, "Strength",
            MeasurementType.DurationMinutes, TaskDifficulty.Standard, 12, "mins", 12,
            EvidenceMode.Manual, false, new[] { "LowImpact" }, "ActionPlanning",
            "12-Minute Resistance Band Basics",
            "12 دقيقة تمارين تقوية باستخدام أحزمة المقاومة",
            "12 मिनट रेजिस्टेंस बैंड से हल्का व्यायाम",
            "Maintains joint stability and functional strength safely at home.",
            "يحافظ على ثبات المفاصل وقوة العضلات بأمان داخل المنزل.",
            "घर पर सुरक्षित रूप से जोड़ों और मांसपेशियों को मजबूत बनाएं।"),

        // ------------------------------------------------------------------------------------------
        // SLEEP ROUTINE (10 Templates)
        // ------------------------------------------------------------------------------------------
        TaskTemplate.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000001"), 1, GoalDomain.SleepRoutine, "Disconnect",
            MeasurementType.DurationMinutes, TaskDifficulty.Starter, 30, "mins", 30,
            EvidenceMode.Manual, false, new[] { "Bedtime", "DigitalDetox" }, "Restructuring",
            "Digital Sundown (Phone Off 30m Pre-Bed)",
            "الغروب الرقمي (إبعاد الهاتف قبل النوم بـ 30 دقيقة)",
            "सोने से 30 मिनट पहले फोन बंद करना",
            "Protects melatonin secretion from blue light stimulation.",
            "يحمي إفراز هرمون الميلاتونين من تأثير الضوء الأزرق للشاشات.",
            "फोन की नीली रोशनी से बचकर गहरी नींद पाएं।"),

        TaskTemplate.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000002"), 1, GoalDomain.SleepRoutine, "Adjust",
            MeasurementType.BooleanCheck, TaskDifficulty.Starter, 1, "check", 2,
            EvidenceMode.Manual, false, new[] { "Bedroom" }, "EnvironmentalRestructuring",
            "Bedroom Climate Check (Optimal 20-22°C)",
            "ضبط تكييف غرفة النوم على درجة حرارة مريحة (20-22°م)",
            "कमरे का तापमान 20-22 डिग्री पर सेट करें",
            "A cooler room core helps your body naturally drop temperature for deep sleep.",
            "الغرفة المعتدلة البرودة تساعد الجسم على خفض حرارته للدخول في نوم عميق.",
            "ठंडा कमरा आपको जल्दी और गहरी नींद में जाने में मदद करता है।"),

        TaskTemplate.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000003"), 1, GoalDomain.SleepRoutine, "Breathe",
            MeasurementType.DurationMinutes, TaskDifficulty.Starter, 5, "mins", 5,
            EvidenceMode.Manual, false, new[] { "Relaxation" }, "StressManagement",
            "5-Minute Evening 4-7-8 Decompression",
            "5 دقائق لتمارين التنفس المسائي المهدئ (4-7-8)",
            "शाम को 5 मिनट गहरी सांस लेने का व्यायाम",
            "Shifts the autonomic nervous system into restorative parasympathetic rest.",
            "يهدئ الجهاز العصبي ويحفز حالة الاسترخاء الطبيعي قبل النوم.",
            "तनाव कम करके दिमाग को शांत करने वाली सांस की तकनीक।"),

        TaskTemplate.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000004"), 1, GoalDomain.SleepRoutine, "Anchor",
            MeasurementType.BooleanCheck, TaskDifficulty.Starter, 1, "check", 2,
            EvidenceMode.Manual, false, new[] { "CaffeineFree" }, "HabitFormation",
            "Caffeine Cutoff (No Coffee After 4 PM)",
            "إيقاف الكافيين والقهوة بعد الساعة 4 مساءً",
            "शाम 4 बजे के बाद चाय/कॉफ़ी न पीना",
            "Prevents adenosine receptor blockage when attempting to fall asleep.",
            "يمنع اضطراب مستقبلات النوم لتسهيل الاستغراق الطبيعي في النوم ليلاً.",
            "रात में नींद की गुणवत्ता सुधारने के लिए कैफीन से बचें।"),

        TaskTemplate.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000005"), 1, GoalDomain.SleepRoutine, "Anchor",
            MeasurementType.BooleanCheck, TaskDifficulty.Standard, 1, "check", 5,
            EvidenceMode.Manual, false, new[] { "Consistency" }, "ActionPlanning",
            "Consistent In-Bed Anchor Window",
            "تحديد موعد ثابت للذهاب إلى الفراش",
            "रोज़ एक ही समय पर बिस्तर पर जाना",
            "Strengthens the internal master clock over successive weeks.",
            "يعزز استقرار الساعة البيولوجية الداخلية عبر الأسابيع المتتالية.",
            "नियमित समय पर सोने से नींद गहरी और बेहतर होती है।"),

        TaskTemplate.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000006"), 1, GoalDomain.SleepRoutine, "Dim",
            MeasurementType.BooleanCheck, TaskDifficulty.Starter, 1, "check", 2,
            EvidenceMode.Manual, false, new[] { "Lighting" }, "EnvironmentalRestructuring",
            "Dim Ambient Lights 1 Hour Pre-Sleep",
            "تعتيم الإضاءة المنزلية قبل النوم بساعة",
            "सोने से 1 घंटा पहले रोशनी धीमी करना",
            "Signals twilight to the brain to initiate winding down.",
            "يرسل إشارة طبيعية للدماغ بأن موعد الراحة قد حان.",
            "धीमी रोशनी दिमाग को सोने के लिए तैयार करती है।"),

        TaskTemplate.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000007"), 1, GoalDomain.SleepRoutine, "Journal",
            MeasurementType.DurationMinutes, TaskDifficulty.Starter, 5, "mins", 5,
            EvidenceMode.Manual, false, new[] { "MentalClarity" }, "CognitiveReframing",
            "5-Minute Tomorrow Task Brain Dump",
            "5 دقائق لكتابة مهام الغد وتفريغ الذهن",
            "5 मिनट में कल के कामों की सूची लिखकर दिमाग शांत करना",
            "Offloads cognitive memory loops that cause sleep-onset insomnia.",
            "يفرغ الأفكار والمشاغل اليومية على الورق لتجنب الأرق والتفكير الزائد.",
            "दिमाग की चिंताएं लिखकर शांति से सोएं।"),

        TaskTemplate.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000008"), 1, GoalDomain.SleepRoutine, "Relax",
            MeasurementType.DurationMinutes, TaskDifficulty.Standard, 15, "mins", 15,
            EvidenceMode.Manual, false, new[] { "WarmBath" }, "SelfCare",
            "Warm Shower / Foot Soak 60m Pre-Bed",
            "دش دافئ أو نقع القدمين قبل النوم بـ 60 دقيقة",
            "सोने से 1 घंटा पहले गर्म पानी से नहाना",
            "Rapid post-shower cooling facilitates deep stage slow-wave sleep.",
            "انخفاض حرارة الجسم بعد الدش الدافئ يسرع الدخول في مراحل النوم العميق.",
            "गर्म पानी से नहाने के बाद शरीर शांत होता है और नींद जल्दी आती है।"),

        TaskTemplate.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000009"), 1, GoalDomain.SleepRoutine, "Darkness",
            MeasurementType.BooleanCheck, TaskDifficulty.Starter, 1, "check", 2,
            EvidenceMode.Manual, false, new[] { "Bedroom" }, "EnvironmentalRestructuring",
            "Blackout Curtain / Sleep Mask Check",
            "التأكد من إحكام حجب أضواء الشارع أو استخدام قناع النوم",
            "कमरे में पूरा अंधेरा सुनिश्चित करें या स्लीप मास्क पहनें",
            "Minimizes micro-arousals caused by external urban night lights.",
            "يمنع الاستيقاظ المفاجئ الناتج عن أضواء الشوارع والأبراج.",
            "बाहरी रोशनी से बचकर बिना रुकावट सोएं।"),

        TaskTemplate.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000010"), 1, GoalDomain.SleepRoutine, "Ritual",
            MeasurementType.BooleanCheck, TaskDifficulty.Starter, 1, "check", 5,
            EvidenceMode.Manual, false, new[] { "Herbal" }, "HabitStacking",
            "Warm Chamomile / Herbal Tea Ritual",
            "تناول كوب شاي البابونج أو الأعشاب المهدئة",
            "सोने से पहले कैमोमाइल या हर्बल चाय पीना",
            "A warm sensory anchor that pairs calm relaxation with bedtime.",
            "طقس مسائي دافئ يربط الراحة والاسترخاء بموعد النوم.",
            "शारीरिक और मानसिक शांति के लिए एक सुखदायक आदत।"),

        // ------------------------------------------------------------------------------------------
        // NUTRITION & HYDRATION (8 Templates)
        // ------------------------------------------------------------------------------------------
        TaskTemplate.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000001"), 1, GoalDomain.Nutrition, "Hydrate",
            MeasurementType.ServingCount, TaskDifficulty.Starter, 1, "glass (500ml)", 2,
            EvidenceMode.Manual, false, new[] { "Morning", "Hydration" }, "HabitStacking",
            "Morning 500ml Hydration Kickstart",
            "كوب ماء كبير (500 مل) فور الاستيقاظ صباحاً",
            "सुबह उठते ही 500 मिली पानी पीना",
            "Replenishes fluids lost overnight under continuous home air-conditioning.",
            "يعوض السوائل المفقودة ليلاً بسبب هواء التكييف الجاف في المنزل.",
            "एसी के कारण रात में हुई पानी की कमी को सुबह तुरंत पूरा करें।"),

        TaskTemplate.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000002"), 1, GoalDomain.Nutrition, "Refill",
            MeasurementType.ServingCount, TaskDifficulty.Starter, 1, "refill", 2,
            EvidenceMode.Manual, false, new[] { "Midday", "Office" }, "EnvironmentalRestructuring",
            "Afternoon Water Bottle Refill Anchor",
            "إعادة ملء زجاجة الماء في فترة الظهيرة",
            "दोपहर में पानी की बोतल दोबारा भरना",
            "Overcomes afternoon energy slumps caused by subtle dehydration.",
            "يتغلب على خمول منتصف اليوم الناتج عن نقص شرب الماء دون أن تشعر.",
            "दोपहर की सुस्ती मिटाने के लिए पर्याप्त पानी पिएं।"),

        TaskTemplate.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000003"), 1, GoalDomain.Nutrition, "Add",
            MeasurementType.ServingCount, TaskDifficulty.Starter, 1, "serving", 5,
            EvidenceMode.Manual, false, new[] { "Lunch", "Micronutrients" }, "Prompting",
            "Add One Fresh Salad or Veggie Serving to Lunch",
            "إضافة طبق سلطة خضراء أو خضار طازجة لوجبة الغداء",
            "दोपहर के भोजन में एक ताज़ा सलाद या सब्ज़ी जोड़ें",
            "Boosts daily dietary fiber and cellular hydration easily.",
            "يعزز الألياف الغذائية والمعادن الأساسية بكل سهولة وبدون تعقيد.",
            "पाचन और ऊर्जा के लिए भोजन में हरी सब्ज़ियाँ शामिल करें।"),

        TaskTemplate.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000004"), 1, GoalDomain.Nutrition, "Focus",
            MeasurementType.DurationMinutes, TaskDifficulty.Starter, 10, "mins", 10,
            EvidenceMode.Manual, false, new[] { "MindfulEating" }, "Restructuring",
            "10-Minute Screen-Free Mindful Meal",
            "تناول وجبة واحدة لمدة 10 دقائق بدون تصفح الهاتف أو الشاشات",
            "बिना फोन या टीवी के 10 मिनट शांति से खाना खाना",
            "Improves satiety cues and prevents unconscious overeating.",
            "يساعد في تمييز إشارات الشبع ويمنع الإفراط في الأكل بسبب التشتت.",
            "ध्यानपूर्वक खाने से पाचन अच्छा होता है और अधिक खाने से बचाव होता है।"),

        TaskTemplate.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000005"), 1, GoalDomain.Nutrition, "Hydrate",
            MeasurementType.ServingCount, TaskDifficulty.Starter, 1, "glass", 2,
            EvidenceMode.Manual, false, new[] { "Commute" }, "ActionPlanning",
            "Pre-Commute Water Glass",
            "كوب ماء قبل مغادرة المنزل أو بدء الطريق",
            "घर से निकलने या यात्रा से पहले एक गिलास पानी",
            "Pre-hydrates your system before facing outdoor heat or traffic.",
            "يرطب الجسم قبل التعرض لحرارة الجو أو زحام الطريق.",
            "गर्मी और यात्रा के दौरान डिहाइड्रेशन से बचने के लिए पानी पिएं।"),

        TaskTemplate.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000006"), 1, GoalDomain.Nutrition, "Swap",
            MeasurementType.BooleanCheck, TaskDifficulty.Standard, 1, "check", 2,
            EvidenceMode.Manual, false, new[] { "Snack" }, "HabitSubstitution",
            "Swap Afternoon Pastry for Raw Almonds or Dates",
            "استبدال حلى المساء بحفنة لوز نيء أو تمر",
            "दोपहर की मिठाई की जगह मुट्ठी भर बादाम या खजूर खाएं",
            "Smooths out glucose spikes for steady late-shift stamina.",
            "يمنع هبوط السكر المفاجئ ويمنح طاقة ثابتة حتى نهاية الدوام.",
            "मीठी चीज़ों से बचकर प्राकृतिक ऊर्जा और पोषण चुनें।"),

        TaskTemplate.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000007"), 1, GoalDomain.Nutrition, "Swap",
            MeasurementType.BooleanCheck, TaskDifficulty.Starter, 1, "check", 2,
            EvidenceMode.Manual, false, new[] { "Beverage" }, "HabitSubstitution",
            "Replace Evening Sugary Soda with Sparkling or Infused Water",
            "استبدال المشروبات الغازية السكرية بماء منكه بالنعناع أو الليمون",
            "सोडा या कोल्ड ड्रिंक की जगह नींबू या पुदीने का पानी पिएं",
            "Saves unnecessary refined sugar while delivering crisp satisfaction.",
            "يوفر كميات كبيرة من السكر المكرر مع إحساس منعش ولذيذ.",
            "अतिरिक्त चीनी से बचें और ताज़ा महसूस करें।"),

        TaskTemplate.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000008"), 1, GoalDomain.Nutrition, "Replenish",
            MeasurementType.ServingCount, TaskDifficulty.Starter, 1, "glass", 3,
            EvidenceMode.Manual, false, new[] { "Electrolyte" }, "SelfCare",
            "Electrolyte Hydration (Pinch of Salt & Lemon in Water)",
            "كوب ماء مع شريحة ليمون ورشة ملح خفيفة لتعويض الأملاح",
            "पानी में चुटकी भर नमक और नींबू मिलाकर इलेक्ट्रोलाइट बनाएं",
            "Rapidly restores electrolyte balance after hot days.",
            "يعوض المعادن والأملاح المفقودة مع التعرق في أيام الحر.",
            "गर्मी में पसीने से निकले आवश्यक खनिजों की भरपाई करें।")
    };
}
