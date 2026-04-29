
$html = @"
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
<meta charset="UTF-8">
<style>
body { font-family: 'Calibri', sans-serif; font-size: 12pt; direction: rtl; }
h1 { color: #1F3864; font-size: 22pt; text-align: center; border-bottom: 3px solid #1F3864; }
h2 { color: #2E75B6; font-size: 16pt; border-bottom: 1px solid #2E75B6; margin-top: 30px; }
h3 { color: #2F5496; font-size: 13pt; }
table { border-collapse: collapse; width: 100%; margin: 10px 0; }
th { background: #1F3864; color: white; padding: 8px; text-align: right; }
td { border: 1px solid #ccc; padding: 7px; text-align: right; }
tr:nth-child(even) { background: #EBF3FB; }
.box { background: #EBF3FB; border-right: 4px solid #2E75B6; padding: 10px; margin: 10px 0; }
.warn { background: #FFF3CD; border-right: 4px solid #FFC107; padding: 8px; margin: 8px 0; }
.ok { background: #D4EDDA; border-right: 4px solid #28A745; padding: 8px; margin: 8px 0; }
code { background: #F4F4F4; padding: 2px 6px; font-family: Consolas; font-size: 10pt; }
pre { background: #1E1E1E; color: #D4D4D4; padding: 12px; font-family: Consolas; font-size: 10pt; direction: ltr; border-radius: 4px; }
.cover { text-align: center; padding: 60px 0; }
.cover h1 { font-size: 28pt; border: none; }
.cover p { font-size: 14pt; color: #555; }
</style>
</head>
<body>

<div class="cover">
  <h1>🏥 Hospital Management System</h1>
  <h1>تقرير تقني شامل</h1>
  <p>Modular Monolith Architecture — .NET 9 + EF Core 9 + MediatR 12</p>
  <p>تاريخ التقرير: 28 أبريل 2026</p>
  <p>الإصدار: 2.0 — النظام المعماري الجديد</p>
</div>

<hr style="page-break-after:always">

<h1>1. نظرة عامة على النظام</h1>

<p>نظام إدارة المستشفيات (HMS) هو نظام متعدد المستأجرين (Multi-Tenant) مبني باستخدام <strong>Modular Monolith Architecture</strong> مع تطبيق مبادئ <strong>Clean Architecture</strong> و <strong>Domain-Driven Design (DDD)</strong>. النظام يدير العمليات الكاملة للمستشفى من استقبال المرضى حتى إدارة الغرف والزيارات.</p>

<div class="ok">
  ✅ <strong>حالة البناء الحالية:</strong> Build Succeeded — 0 Errors عبر جميع المشاريع الـ 34
</div>

<h2>1.1 التقنيات المستخدمة</h2>
<table>
<tr><th>التقنية</th><th>الإصدار</th><th>الغرض</th></tr>
<tr><td>.NET</td><td>9.0</td><td>إطار العمل الأساسي</td></tr>
<tr><td>Entity Framework Core</td><td>9.0</td><td>ORM لقاعدة البيانات</td></tr>
<tr><td>MediatR</td><td>12.1.1</td><td>CQRS + Domain Events</td></tr>
<tr><td>FluentValidation</td><td>12.1.1</td><td>التحقق من صحة البيانات</td></tr>
<tr><td>JWT Bearer</td><td>9.0</td><td>المصادقة والتفويض</td></tr>
<tr><td>SignalR</td><td>ASP.NET Core</td><td>الإشعارات الفورية</td></tr>
<tr><td>Serilog</td><td>Latest</td><td>تسجيل الأحداث</td></tr>
<tr><td>SQL Server</td><td>2022</td><td>قاعدة البيانات</td></tr>
<tr><td>QuestPDF</td><td>Community</td><td>إنشاء ملفات PDF</td></tr>
</table>

<h1>2. البنية المعمارية</h1>

<h2>2.1 Modular Monolith Pattern</h2>
<p>النظام يعمل كـ <strong>Monolith واحد</strong> لكنه مقسم داخلياً إلى <strong>وحدات (Modules) مستقلة</strong> لكل نطاق عمل (Bounded Context). كل وحدة لها حدودها الخاصة ولا تتواصل مع غيرها مباشرة — بل عبر <strong>Domain Events</strong>.</p>

<div class="box">
  <strong>الفرق عن Microservices:</strong> الكود يعمل في عملية واحدة (Process واحد) مما يتجنب تعقيدات الشبكة، لكن الكود منظم مثل الـ Microservices تماماً.
</div>

<h2>2.2 طبقات Clean Architecture لكل وحدة</h2>
<pre>
/Module
├── Domain/          ← قواعد الأعمال الصافية (لا EF، لا Framework)
├── Application/     ← حالات الاستخدام (MediatR Commands/Queries)
├── Infrastructure/  ← التنفيذ التقني (EF Configs، Services)
└── API/             ← Controllers + Endpoints
</pre>

<h2>2.3 هيكل المشاريع الكامل</h2>
<table>
<tr><th>المسار</th><th>النوع</th><th>الوظيفة</th></tr>
<tr><td>src/HMS.API</td><td>🚀 Entry Point</td><td>نقطة دخول التطبيق — Program.cs + Controllers</td></tr>
<tr><td>src/HMS.Persistence</td><td>🗄️ Composition Root</td><td>HmsDbContext الموحد + EF Migrations</td></tr>
<tr><td>src/SharedKernel/Domain</td><td>⚙️ Primitives</td><td>BaseEntity، TenantEntity، IDomainEvent، DomainException</td></tr>
<tr><td>src/SharedKernel/Application</td><td>⚙️ Behaviors</td><td>LoggingBehavior، ValidationBehavior (MediatR Pipeline)</td></tr>
<tr><td>src/SharedKernel/Infrastructure</td><td>⚙️ Base Infra</td><td>ApplicationDbContextBase، AuditInterceptor، TenantProvider</td></tr>
<tr><td>src/Modules/Identity/*</td><td>🔐 Module</td><td>المصادقة: Users، Roles، Permissions، JWT</td></tr>
<tr><td>src/Modules/Patients/*</td><td>👤 Module</td><td>إدارة بيانات المرضى</td></tr>
<tr><td>src/Modules/Rooms/*</td><td>🏠 Module</td><td>إدارة الغرف وتخصيصها</td></tr>
<tr><td>src/Modules/Intake/*</td><td>📋 Module</td><td>الاستقبال وتسجيل الدخول للمرضى</td></tr>
<tr><td>src/Modules/Visits/*</td><td>🩺 Module</td><td>إدارة الزيارات الطبية</td></tr>
<tr><td>src/Modules/Notifications/*</td><td>🔔 Module</td><td>الإشعارات الفورية عبر SignalR</td></tr>
<tr><td>src/HMS.Infrastructure (Legacy)</td><td>⚠️ Legacy</td><td>الكود القديم — جاري استبداله تدريجياً</td></tr>
<tr><td>src/HMS.Application (Legacy)</td><td>⚠️ Legacy</td><td>Handlers القديمة — جاري الاستبدال</td></tr>
</table>

<h1>3. قاعدة البيانات والـ Multi-Tenancy</h1>

<h2>3.1 استراتيجية التعدد (Multi-Tenancy)</h2>
<p>النظام يستخدم <strong>Shared Database + Shared Schema</strong> مع تمييز البيانات عبر عمود <code>TenantId</code> في كل جدول. كل طلب يحمل هوية المستأجر (Tenant) عبر:</p>

<table>
<tr><th>المصدر</th><th>الأولوية</th><th>التفاصيل</th></tr>
<tr><td>JWT Claim: <code>orgId</code></td><td>1 (الأعلى)</td><td>للمستخدمين العاديين</td></tr>
<tr><td>Header: <code>X-Tenant-Id</code></td><td>2</td><td>للـ Super Admin فقط</td></tr>
<tr><td>Request Body TenantId</td><td>3</td><td>كـ Override للـ Super Admin</td></tr>
</table>

<div class="box">
  <strong>Global Query Filters:</strong> كل Entity يرث من <code>TenantEntity</code> له فلتر تلقائي في EF Core يضيف <code>WHERE TenantId = @currentTenantId</code> على كل استعلام. هذا يمنع تسرب بيانات المستأجرين.
</div>

<h2>3.2 مخطط قاعدة البيانات (Schemas)</h2>
<table>
<tr><th>Schema</th><th>الجداول</th><th>الوحدة المسؤولة</th></tr>
<tr><td><code>identity.*</code></td><td>Users, Roles, Permissions, UserRoles, RolePermissions, RefreshTokens, UserSessions</td><td>Identity Module</td></tr>
<tr><td><code>patients.*</code></td><td>Patients</td><td>Patients Module</td></tr>
<tr><td><code>rooms.*</code></td><td>Rooms, RoomAssignments</td><td>Rooms Module</td></tr>
<tr><td><code>intake.*</code></td><td>Intakes, IntakeEmergencyContacts, IntakeInsurance, IntakeFlags</td><td>Intake Module</td></tr>
<tr><td><code>visits.*</code></td><td>Visits</td><td>Visits Module</td></tr>
<tr><td><code>shared.*</code></td><td>__EFMigrationsHistory</td><td>HMS.Persistence</td></tr>
</table>

<h2>3.3 HmsDbContext — قلب النظام</h2>
<p>الملف: <code>src/HMS.Persistence/HmsDbContext.cs</code></p>
<p>هذا الـ DbContext الوحيد في النظام. يطبق <strong>جميع</strong> واجهات الوحدات:</p>
<pre>
public sealed class HmsDbContext :
    ApplicationDbContextBase,     // Global filters + Audit
    IIdentityDbContext,           // واجهة وحدة Identity
    IVisitsDbContext,             // واجهة وحدة Visits
    IRoomsDbContext,              // واجهة وحدة Rooms
    IIntakeDbContext              // واجهة وحدة Intake
</pre>

<div class="box">
  <strong>لماذا DbContext واحد؟</strong> لأن جميع الوحدات في نفس العملية وقاعدة البيانات. استخدام DbContext منفصل لكل وحدة سيؤدي إلى مشاكل في الـ Transactions عبر الوحدات.
</div>

<h1>4. تدفق العمليات الرئيسية</h1>

<h2>4.1 تدفق تسجيل دخول مريض (Intake Submission)</h2>
<pre>
1. Client → POST /api/intake/submit
2. SubmitIntakeCommandHandler يُنفَّذ
3. يتم إنشاء/تحديث Patient
4. PatientIntake.Submit() يُغيّر الحالة لـ Submitted
5. IntakeSubmittedEvent يُرفع (Domain Event)
6. AuditAndDomainEventInterceptor يلتقط الـ Event بعد SaveChanges
7. MediatR يُوجّه الـ Event لـ IntakeSubmittedEventHandler (في Visits Module)
8. IntakeSubmittedEventHandler:
   ├── يحجز غرفة بـ UPDLOCK (منع التزامن)
   ├── يختار دكتور أقل حمل
   ├── يُنشئ Visit جديدة
   ├── يُنشئ RoomAssignment
   └── يُحدّث الـ Intake لـ ConvertedToVisit
9. VisitCreatedEvent يُرفع
10. VisitCreatedNotificationHandler → SignalR → Dashboard يتحدث فوراً
</pre>

<h2>4.2 معالجة التزامن (Concurrency)</h2>
<p>مشكلة: مريضان في نفس اللحظة قد يُخصَّص لهما نفس الغرفة.</p>
<p>الحل المطبق: <strong>Pessimistic Locking عبر SQL UPDLOCK</strong></p>
<pre>
SELECT TOP 1 r.* FROM rooms.Rooms r WITH (UPDLOCK, ROWLOCK)
WHERE r.TenantId = {0}
  AND r.IsOccupied = 0
  AND r.IsDeleted  = 0
ORDER BY r.RoomNumber
</pre>
<p>بالإضافة: <code>Room.RowVersion</code> كـ Optimistic Concurrency Token كخط دفاع ثانٍ.</p>

<h1>5. نمط CQRS و Domain Events</h1>

<h2>5.1 CQRS Pattern</h2>
<table>
<tr><th>النوع</th><th>الواجهة</th><th>المعالج</th><th>مثال</th></tr>
<tr><td>Command</td><td>ICommand&lt;TResult&gt;</td><td>ICommandHandler&lt;TCommand, TResult&gt;</td><td>SubmitIntakeCommand</td></tr>
<tr><td>Query</td><td>IQuery&lt;TResult&gt;</td><td>IQueryHandler&lt;TQuery, TResult&gt;</td><td>GetVisitsQuery</td></tr>
<tr><td>Domain Event</td><td>IDomainEvent</td><td>INotificationHandler&lt;TEvent&gt;</td><td>IntakeSubmittedEvent</td></tr>
</table>

<h2>5.2 MediatR Pipeline</h2>
<pre>
Request → LoggingBehavior → ValidationBehavior → Handler → Response
</pre>
<ul>
<li><strong>LoggingBehavior:</strong> يسجل كل طلب مع وقت التنفيذ</li>
<li><strong>ValidationBehavior:</strong> يُشغّل FluentValidation قبل الـ Handler — يرفع ValidationException تلقائياً</li>
</ul>

<h2>5.3 Domain Events Flow</h2>
<pre>
Domain Entity (e.g. PatientIntake.Submit())
    └── RaiseDomainEvent(new IntakeSubmittedEvent(...))
         └── مخزن في List&lt;IDomainEvent&gt; داخل BaseEntity

EF Core SaveChanges()
    └── AuditAndDomainEventInterceptor.SavedChangesAsync()
         └── يجمع كل Events من كل Entities
              └── IPublisher.Publish(event) → MediatR يوزّعها للـ Handlers
</pre>

<h1>6. نظام الأمان والصلاحيات</h1>

<h2>6.1 JWT Authentication</h2>
<table>
<tr><th>الـ Claim</th><th>المحتوى</th></tr>
<tr><td><code>sub</code></td><td>UserId</td></tr>
<tr><td><code>orgId</code></td><td>TenantId (المستأجر)</td></tr>
<tr><td><code>email</code></td><td>البريد الإلكتروني</td></tr>
<tr><td><code>roles</code></td><td>قائمة الأدوار</td></tr>
<tr><td><code>permissions</code></td><td>الصلاحيات المحددة</td></tr>
</table>

<div class="warn">
  ⚠️ <strong>ملاحظة مهمة:</strong> Claim اسمه <code>orgId</code> وليس <code>tenantId</code>. هذا موحّد في جميع أجزاء النظام الجديد. Legacy code كان يستخدم <code>tenantId</code> — تم تصحيحه.
</div>

<h2>6.2 نظام الصلاحيات</h2>
<pre>
User ──→ UserRole ──→ Role ──→ RolePermission ──→ Permission
</pre>
<p>يتم تخزين الصلاحيات في Cache (IPermissionCacheService) لتجنب استعلامات قاعدة البيانات في كل طلب.</p>

<h1>7. وحدة Notifications والـ SignalR</h1>

<h2>7.1 آلية العمل</h2>
<pre>
Domain Event يُرفع (مثل VisitCreatedEvent)
    └── VisitCreatedNotificationHandler يُشغَّل
         └── INotificationsService.BroadcastToTenantAsync()
              └── SignalRNotificationsService
                   ├── DashboardHub → مجموعة "tenant-{TenantId}"
                   └── NotificationHub → مجموعة "tenant-{TenantId}"
</pre>

<h2>7.2 Hubs المتاحة</h2>
<table>
<tr><th>Hub</th><th>المسار</th><th>الغرض</th></tr>
<tr><td>DashboardHub</td><td>/hubs/dashboard</td><td>تحديثات لوحة التحكم الفورية</td></tr>
<tr><td>NotificationHub</td><td>/hubs/notifications</td><td>الإشعارات العامة</td></tr>
</table>

<h1>8. الـ EF Core Migration</h1>

<h2>8.1 Migration المُنشأة</h2>
<div class="ok">
  ✅ تم إنشاء: <code>20260428033725_InitialModularSchema</code> — حجم الملف: 33KB
</div>

<h2>8.2 تطبيق الـ Migration على قاعدة البيانات</h2>
<pre>
dotnet ef database update \
  --project src/HMS.Persistence/HMS.Persistence.csproj \
  --startup-project src/HMS.API/HMS.API.csproj
</pre>

<h2>8.3 DesignTimeDbContextFactory</h2>
<p>الملف: <code>src/HMS.Persistence/HmsDbContextDesignTimeFactory.cs</code></p>
<p>يُمكّن EF Core من إنشاء DbContext وقت التطوير (Design Time) بدون تشغيل التطبيق الكامل. يقرأ Connection String من <code>HMS.API/appsettings.json</code>.</p>

<h1>9. الكود القديم (Legacy) والتحول التدريجي</h1>

<div class="warn">
  ⚠️ <strong>تحذير:</strong> المشاريع التالية لا تزال موجودة للتوافق الخلفي (Backward Compatibility) ويجب إزالتها تدريجياً:
  <ul>
    <li><code>src/HMS.Application</code> — Handlers القديمة</li>
    <li><code>src/HMS.Infrastructure</code> — Services القديمة</li>
    <li><code>src/HMS.Domain</code> — Entities القديمة</li>
  </ul>
</div>

<h2>9.1 خطة إزالة الكود القديم</h2>
<table>
<tr><th>الخطوة</th><th>الإجراء</th><th>الأولوية</th></tr>
<tr><td>1</td><td>تطبيق Migration على قاعدة البيانات</td><td>🔴 فورية</td></tr>
<tr><td>2</td><td>اختبار Login + Intake Submit من النهاية للنهاية</td><td>🔴 فورية</td></tr>
<tr><td>3</td><td>نقل Controllers للـ Modules الجديدة</td><td>🟡 متوسطة</td></tr>
<tr><td>4</td><td>حذف HMS.Application القديم</td><td>🟡 متوسطة</td></tr>
<tr><td>5</td><td>حذف HMS.Infrastructure القديم</td><td>🟢 مستقبلية</td></tr>
</table>

<h1>10. نقاط القوة والملاحظات المعمارية</h1>

<h2>10.1 قرارات معمارية مهمة</h2>
<table>
<tr><th>القرار</th><th>السبب</th></tr>
<tr><td>DbContext واحد موحّد</td><td>Transactions عبر الوحدات، أداء أفضل</td></tr>
<tr><td>Domain Events بدلاً من Direct Service Calls</td><td>فصل Intake عن Visits — كل وحدة مستقلة</td></tr>
<tr><td>UPDLOCK SQL بدلاً من Lock() في الكود</td><td>الأمان من Race Conditions على مستوى قاعدة البيانات</td></tr>
<tr><td>FluentValidation في MediatR Pipeline</td><td>Validation مركزي — لا تكرار في كل Handler</td></tr>
<tr><td>RowVersion على Room</td><td>طبقة ثانية من الحماية ضد Double Booking</td></tr>
<tr><td>orgId في JWT بدلاً من tenantId</td><td>توحيد المعيار — منع الارتباك في جميع الأماكن</td></tr>
</table>

<h2>10.2 مسار API الكامل للـ Endpoints</h2>
<table>
<tr><th>Endpoint</th><th>Method</th><th>الوظيفة</th><th>الـ Handler</th></tr>
<tr><td>/api/auth/login</td><td>POST</td><td>تسجيل الدخول</td><td>LoginCommandHandler (Identity)</td></tr>
<tr><td>/api/auth/refresh</td><td>POST</td><td>تجديد التوكن</td><td>Legacy Handler</td></tr>
<tr><td>/api/intake/submit</td><td>POST</td><td>تسجيل مريض</td><td>SubmitIntakeCommandHandler</td></tr>
<tr><td>/api/visits</td><td>GET</td><td>قائمة الزيارات</td><td>GetVisitsQueryHandler</td></tr>
<tr><td>/api/patients</td><td>CRUD</td><td>إدارة المرضى</td><td>Legacy Handlers</td></tr>
<tr><td>/api/rooms</td><td>CRUD</td><td>إدارة الغرف</td><td>Legacy + AssignRoomCommand</td></tr>
<tr><td>/health</td><td>GET</td><td>فحص صحة التطبيق</td><td>Health Checks</td></tr>
<tr><td>/swagger</td><td>GET</td><td>توثيق API</td><td>Swagger UI</td></tr>
<tr><td>/hubs/dashboard</td><td>WS</td><td>Dashboard SignalR</td><td>DashboardHub</td></tr>
<tr><td>/hubs/notifications</td><td>WS</td><td>إشعارات SignalR</td><td>NotificationHub</td></tr>
</table>

<h1>11. الخطوات التالية الفورية</h1>

<div class="ok">
<strong>الخطوة 1 — تطبيق قاعدة البيانات (ضروري الآن):</strong>
<pre>
dotnet ef database update --project src\HMS.Persistence --startup-project src\HMS.API
</pre>
</div>

<div class="box">
<strong>الخطوة 2 — تشغيل التطبيق:</strong>
<pre>
cd src\HMS.API
dotnet run
</pre>
ثم اختبار: <code>POST /api/auth/login</code> بـ <code>{"email":"...", "password":"..."}</code>
</div>

<div class="box">
<strong>الخطوة 3 — اختبار التدفق الكامل:</strong>
<ol>
<li>Login → احصل على JWT Token</li>
<li>POST /api/intake/submit → يُنشئ مريض + Intake + Visit + Room Assignment</li>
<li>تحقق من Dashboard عبر SignalR يتحدث فوراً</li>
<li>GET /api/visits → تأكد أن الزيارة موجودة</li>
</ol>
</div>

<hr>
<p style="text-align:center; color:#888; font-size:10pt">
HMS Technical Report v2.0 | Modular Monolith Architecture | Generated 2026-04-28
</p>

</body>
</html>
"@

$outPath = "D:\multi tenant\multiTenanti\HMS_System_Report.html"
$html | Out-File -FilePath $outPath -Encoding UTF8
Write-Host "HTML Report saved to: $outPath"

# Try to convert to Word using COM if Word is installed
try {
    $word = New-Object -ComObject Word.Application
    $word.Visible = $false
    $doc = $word.Documents.Open($outPath)
    $docxPath = "D:\multi tenant\multiTenanti\HMS_System_Report.docx"
    $doc.SaveAs([ref]$docxPath, [ref]16)  # 16 = wdFormatDocx
    $doc.Close()
    $word.Quit()
    Write-Host "Word document saved to: $docxPath"
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($word) | Out-Null
} catch {
    Write-Host "Word not installed or COM failed. Please open the HTML file in Word manually."
    Write-Host "Error: $_"
}
