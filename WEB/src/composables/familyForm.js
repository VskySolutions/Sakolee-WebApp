// A blank Family form — one canonical shape shared by the family module's Create/Edit drawer
// (FamilyFormDrawer) and read-only View drawer (FamilyViewDrawer).
export const blankSecondaryContact = () => ({
  firstName: "",
  lastName: "",
  email: "",
  phone: "",
  relation: "",
  isBillingContact: false,
  isAuthorizedToPickUpStudent: false
});

export const blankFamilyForm = () => ({
  familyName: "",
  studioLocationId: null,
  studioLocationName: "", // display-only, never sent
  familyStatusId: null,
  source: "",
  referralName: "",
  // Primary contact — inlined on the Family record itself (see FamiliesController remarks).
  firstName: "",
  lastName: "",
  email: "",
  relation: "",
  homePhone: "",
  workPhone: "",
  cellPhone: "",
  otherPhone: "",
  fax: "",
  isBillingContact: true,
  isAuthorizedToPickUpStudent: true,
  secondaryContact: null,
  address1: "",
  address2: "",
  city: "",
  state: "",
  zipCode: null,
  emergencyContactPerson: "",
  emergencyPhone: "",
  healthInsuranceCarrier: "",
  active: true,
  // New students to enrol under this family on save — see the "Add Student(s)" section. Never
  // pre-filled from an existing family's own students (those come back read-only via the detail's
  // `students`, kept separately by each drawer).
  newStudents: []
});

// Maps a familyApi.get() detail onto the form shape above (everything except newStudents).
export const familyFormFromDetail = (detail) => {
  const primary = (detail.contacts || []).find((c) => c.isPrimaryContact) || null;
  const secondary = (detail.contacts || []).find((c) => !c.isPrimaryContact) || null;

  return {
    familyName: detail.familyName || "",
    studioLocationId: detail.studioLocationId || null,
    studioLocationName: detail.studioLocationName || "",
    familyStatusId: detail.familyStatusId || null,
    source: detail.source || "",
    referralName: detail.referralName || "",
    firstName: primary?.firstName || "",
    lastName: primary?.lastName || "",
    email: primary?.email || "",
    relation: primary?.relation || "",
    homePhone: detail.homePhone || "",
    workPhone: detail.workPhone || "",
    cellPhone: primary?.phone || "",
    otherPhone: detail.otherPhone || "",
    fax: detail.fax || "",
    isBillingContact: !!primary?.isBillingContact,
    isAuthorizedToPickUpStudent: !!primary?.isAuthorizedToPickUpStudent,
    secondaryContact: secondary
      ? {
        firstName: secondary.firstName || "",
        lastName: secondary.lastName || "",
        email: secondary.email || "",
        phone: secondary.phone || "",
        relation: secondary.relation || "",
        isBillingContact: !!secondary.isBillingContact,
        isAuthorizedToPickUpStudent: !!secondary.isAuthorizedToPickUpStudent
      }
      : null,
    address1: detail.address1 || "",
    address2: detail.address2 || "",
    city: detail.city || "",
    state: detail.state || "",
    zipCode: detail.zipCode ?? null,
    emergencyContactPerson: detail.emergencyContactPerson || "",
    emergencyPhone: detail.emergencyPhone || "",
    healthInsuranceCarrier: detail.healthInsuranceCarrier || "",
    active: detail.active
  };
};
