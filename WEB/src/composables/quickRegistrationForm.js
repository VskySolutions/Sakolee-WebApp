// Quick Registration — the single-pass intake form from the Admin Portal prototype
// ("view-quick-registration"): family, two contacts, address, one student, one class and a
// payment schedule, all captured on one page.
//
// There is still no Family entity/endpoint, so the family/studio metadata (heardAbout,
// referralName, studioLocation), the second contact, and payment stay UI-only — nothing for them
// to save to. But the student profile, address, emergency contact, health insurance and class
// enrollment all now have a real home on Student/Person (see StudentsController), so
// toCreateStudentRequest() below maps those onto a real studentApi.create() call.

// The prototype's option lists, kept here rather than in the template so the page stays readable.
// Each of these is a placeholder for a real lookup — Studio Location, Class and the rest will come
// from their own endpoints (Family Statuses is the pattern; see /familystatus).
export const HEARD_ABOUT_OPTIONS = [
  { label: "Google Search", value: "Google Search" },
  { label: "Instagram / Facebook", value: "Social Media" },
  { label: "Word of Mouth", value: "Word of Mouth" },
  { label: "Flyer / Banner", value: "Print Flyer" }
];

export const STUDIO_LOCATION_OPTIONS = [
  { label: "North Studio", value: "North Studio" },
  { label: "Downtown Campus", value: "Downtown Campus" },
  { label: "Westside Academy", value: "Westside Academy" }
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

export const blankQuickRegistrationForm = () => ({
  // Step 1 — Family & Studio
  heardAbout: "",
  referralName: "",
  studioLocation: "",
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

  // Step 4 — Student
  student: {
    firstName: "",
    lastName: "",
    birthDate: "",
    gender: "",
    tshirtSize: "",
    gradeLevel: "",
    medicalNotes: ""
  },

  // Step 5 — Enrollment
  className: "",
  enrollmentDate: "",
  trialEnrollment: false,
  emailInstructorNotice: true,

  // Step 6 — Payment. Card details are deliberately absent: see the page's payment section.
  paymentMethod: "card",
  acceptPolicies: false
});

// Shapes the form for the intake endpoint. Written now so the page has one place to change when
// the endpoint exists, rather than the request being assembled inline at the call site.
export const toQuickRegistrationPayload = (form) => ({
  family: {
    name: form.familyName,
    heardAbout: form.heardAbout,
    referralName: form.referralName || null,
    studioLocation: form.studioLocation
  },
  contacts: [form.primaryContact, form.secondaryContact]
    // The second contact is optional — drop it unless a name was entered.
    .filter((c) => c.firstName.trim() || c.lastName.trim())
    .map((c) => ({
      firstName: c.firstName,
      lastName: c.lastName,
      relation: c.relation,
      email: c.email || null,
      phone: c.phone || null,
      countryCode: c.phoneCountry || null
    })),
  address: {
    street: form.streetAddress,
    city: form.city,
    state: form.state,
    postalCode: form.postalCode
  },
  emergency: {
    contactName: form.emergencyContactName,
    phone: form.emergencyPhone || null,
    countryCode: form.emergencyPhoneCountry || null,
    insuranceCarrier: form.insuranceCarrier || null
  },
  student: {
    firstName: form.student.firstName,
    lastName: form.student.lastName,
    birthDate: form.student.birthDate || null,
    gender: form.student.gender || null,
    tshirtSize: form.student.tshirtSize || null,
    gradeLevel: form.student.gradeLevel || null,
    medicalNotes: form.student.medicalNotes || null
  },
  enrollment: {
    className: form.className,
    enrollmentDate: form.enrollmentDate || null
  },
  // The gateway returns a token for card/ACH; only the choice of method belongs in our payload.
  payment: { method: form.paymentMethod }
});

// Maps the parts of the form that DO have a real backing field today onto a CreateStudentRequest
// (see StudentsController.Create). The primary contact's email/phone become the student's login
// email/cell phone — there is no separate guardian-login concept in this domain, so the adult who
// registers the child is who signs in. `classId` is the real Class.Id selected in the page (fetched
// from classApi — the composable's own CLASS_OPTIONS are demo labels, not real ids).
export const toCreateStudentRequest = (form, classId) => ({
  firstName: form.student.firstName,
  lastName: form.student.lastName,
  familyName: form.familyName || null,
  birthDate: form.student.birthDate || null,
  gender: form.student.gender || null,
  tShirtSize: form.student.tshirtSize || null,
  gradeLevel: form.student.gradeLevel || null,
  specialNeeds: form.student.medicalNotes || null,
  email: form.primaryContact.email,
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
