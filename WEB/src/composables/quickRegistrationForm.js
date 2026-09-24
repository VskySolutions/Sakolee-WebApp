// Quick Registration — the single-pass intake form from the Admin Portal prototype
// ("view-quick-registration"): family, two contacts, address, one or more students (sharing one
// class enrollment) and a payment schedule, all captured on one page.
//
// Family/studio metadata, both contacts, and every student now all have a real home
// (FamiliesController + StudentsController — see quick_registration.vue's submit handler, which
// calls familyApi.create() then studentApi.createBulk() with the new family's id as familyId on
// every student). Only payment stays UI-only — there is still no billing intake to save it to.

// The prototype's option lists, kept here rather than in the template so the page stays readable.
// Studio Location and Class are real lookups, fetched by the page itself (locationApi/classApi) —
// these two stay free text (Family.Source/Family.Type or FamilyContact.Relation on the backend).
export const HEARD_ABOUT_OPTIONS = [
  { label: "Google Search", value: "Google Search" },
  { label: "Instagram / Facebook", value: "Social Media" },
  { label: "Word of Mouth", value: "Word of Mouth" },
  { label: "Flyer / Banner", value: "Print Flyer" }
];

export const RELATION_OPTIONS = [
  { label: "Mother", value: "Mother" },
  { label: "Father", value: "Father" },
  { label: "Guardian", value: "Guardian" }
];

export const GENDER_OPTIONS = [
  { label: "Female", value: "Female" },
  { label: "Male", value: "Male" },
  { label: "Non-Binary / Other", value: "Non-Binary / Other" }
];

export const TSHIRT_SIZE_OPTIONS = [
  { label: "Child S", value: "Child S" },
  { label: "Child M", value: "Child M" },
  { label: "Child L", value: "Child L" },
  { label: "Adult S", value: "Adult S" },
  { label: "Adult M", value: "Adult M" }
];

export const CLASS_OPTIONS = [
  { label: "Ballet II — Mon & Wed 4:00 PM ($120/mo)", value: "Ballet II" },
  { label: "Jazz & Tap Beg — Tue 5:30 PM ($95/mo)", value: "Jazz & Tap Beg" },
  { label: "Hip Hop Juniors — Thu 6:00 PM ($90/mo)", value: "Hip Hop Juniors" },
  { label: "Contemporary Ensemble — Sat 10:00 AM ($130/mo)", value: "Contemporary Ensemble" }
];

export const PAYMENT_METHOD_OPTIONS = [
  { label: "Credit Card (Stripe Gateway)", value: "card" },
  { label: "eCheck / Bank Draft (ACH)", value: "ach" },
  { label: "Manual Invoice", value: "invoice" }
];

const blankContact = (relation) => ({
  firstName: "",
  lastName: "",
  relation,
  email: "",
  phone: "",
  phoneCountry: null,
  // Prototype shows this only for the primary contact — carried on both for a symmetric shape.
  textOptIn: true
});

// A stable per-row key (not the eventual Student.Id, which doesn't exist until submit) — lets the
// "Remove" button drop a row from the middle of the list without Vue reusing another row's input
// state under it.
let nextStudentKey = 1;
export const blankStudent = () => ({
  key: nextStudentKey++,
  firstName: "",
  lastName: "",
  email: "",
  birthDate: "",
  gender: "",
  tshirtSize: "",
  gradeLevel: "",
  medicalNotes: ""
});

export const blankQuickRegistrationForm = () => ({
  // Step 1 — Family & Studio
  heardAbout: "",
  referralName: "",
  studioLocationId: null,
  familyName: "",

  // Step 2 — Contacts. The prototype captures a primary and an optional second contact.
  primaryContact: blankContact("Mother"),
  secondaryContact: blankContact("Father"),

  // Step 3 — Address & emergency
  streetAddress: "",
  city: "",
  state: "",
  postalCode: "",
  emergencyContactName: "",
  emergencyPhone: "",
  emergencyPhoneCountry: null,
  insuranceCarrier: "",

  // Step 4 — Student(s). A family can enrol more than one child in one pass — see the "Add Another
  // Student" button — so this is a list, starting with one blank entry. Each student gets its own
  // email, distinct from the contacts' AND from every other student's — the family contact created
  // in Step 2 now gets its own real login (FamiliesController), so a student can no longer reuse the
  // primary contact's email for its own login (StudentsController mints one per student too, and
  // Users.Email is unique).
  students: [blankStudent()],

  // Step 5 — Enrollment
  className: "",
  enrollmentDate: "",
  trialEnrollment: false,
  emailInstructorNotice: true,

  // Step 6 — Payment. Card details are deliberately absent: see the page's payment section.
  paymentMethod: "card",
  acceptPolicies: false
});

// Maps Steps 1–3 onto a CreateFamilyRequest (see FamiliesController.Create). Submitted first — the
// family this student enrols under has to exist before the student can name it as its parent.
export const toCreateFamilyRequest = (form) => {
  const secondary = form.secondaryContact;
  // The second contact is optional; FamiliesController requires a full contact (name + email) for
  // any slot it's given, so a partially-filled secondary contact is dropped rather than submitted
  // half-complete.
  const hasSecondary = !!(secondary.firstName.trim() && secondary.lastName.trim() && secondary.email.trim());
  const hasAddress = form.streetAddress || form.city || form.state || form.postalCode;

  // ZipCode is a plain int on the backend (Parents.ZipCode) — no support for a non-numeric or ZIP+4 code.
  const zip = parseInt(form.postalCode, 10);

  return {
    familyName: form.familyName,
    studioLocationId: form.studioLocationId || null,
    source: form.heardAbout || null,
    referralName: form.referralName || null,
    // Primary contact — inlined directly on CreateFamilyRequest (see FamiliesController remarks).
    firstName: form.primaryContact.firstName,
    lastName: form.primaryContact.lastName,
    email: form.primaryContact.email,
    relation: form.primaryContact.relation || null,
    cellPhone: form.primaryContact.phone || null,
    isBillingContact: true,
    isAuthorizedToPickUpStudent: true,
    secondaryContact: hasSecondary
      ? {
        firstName: secondary.firstName,
        lastName: secondary.lastName,
        email: secondary.email,
        phone: secondary.phone || null,
        relation: secondary.relation || null,
        isBillingContact: false,
        isAuthorizedToPickUpStudent: true
      }
      : null,
    emergencyContactPerson: form.emergencyContactName || null,
    emergencyPhone: form.emergencyPhone || null,
    healthInsuranceCarrier: form.insuranceCarrier || null,
    address1: hasAddress ? form.streetAddress || null : null,
    city: hasAddress ? form.city || null : null,
    state: hasAddress ? form.state || null : null,
    zipCode: hasAddress && !Number.isNaN(zip) ? zip : null
  };
};

// Maps ONE Step-4 student (plus the shared Step 5/6 enrollment fields) onto a CreateStudentRequest
// (see StudentsController.Create) — called once per entry in form.students, since a family can
// enrol more than one child in one pass. Each student's own email (Step 4) becomes its login —
// kept distinct from the contacts' emails AND from every other student's, since a family contact
// now gets its own real login too (FamiliesController) and Users.Email is unique.
// `familyId` is the id returned by the toCreateFamilyRequest() call this page makes first;
// `classId` is the real Class.Id selected in the page (fetched from classApi — the composable's
// own CLASS_OPTIONS are demo labels, not real ids), shared by every student in this registration.
export const toCreateStudentRequest = (student, form, classId, familyId) => ({
  familyId: familyId || null,
  firstName: student.firstName,
  lastName: student.lastName,
  familyName: form.familyName || null,
  birthDate: student.birthDate || null,
  gender: student.gender || null,
  tShirtSize: student.tshirtSize || null,
  gradeLevel: student.gradeLevel || null,
  specialNeeds: student.medicalNotes || null,
  email: student.email,
  cellPhone: form.primaryContact.phone || null,
  classId: classId || null,
  admissionDate: form.enrollmentDate || null,
  healthInsuranceCarrier: form.insuranceCarrier || null,
  emergencyContactName: form.emergencyContactName || null,
  emergencyContactNumber: form.emergencyPhone || null,
  address: form.streetAddress || form.city || form.state || form.postalCode
    ? {
      addressLine1: form.streetAddress || null,
      cityName: form.city || null,
      stateName: form.state || null,
      postalCode: form.postalCode || null
    }
    : null
});
