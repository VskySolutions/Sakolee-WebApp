// The address shape AppAddressFields edits and the API's AddressInput / AddressResponse carry — shared by the
// forms that store an address on the Addresses table (Family; Tenant keeps its own copy of the same shape).

// One blank address, for new forms and resets.
export const blankAddress = () => ({
  countryCode: null,
  countryName: null,
  stateCode: null,
  stateName: null,
  cityName: null,
  postalCode: "",
  addressLine1: "",
  addressLine2: "",
  landmark: "",
  buildingName: "",
  floorNumber: "",
  unitNumber: ""
});

// An AddressResponse from the API onto the blank shape (null → blank), so every key AppAddressFields binds exists.
export const addressFromResponse = (address) => {
  const blank = blankAddress();
  if (!address) return blank;
  return Object.fromEntries(Object.keys(blank).map((key) => [key, address[key] ?? blank[key]]));
};

// True when any field holds a value — an untouched address block is not sent at all.
export const hasAddressValue = (address) =>
  !!address && Object.keys(blankAddress()).some((key) => `${address[key] ?? ""}`.trim() !== "");

// The address as display lines: street lines, then "City, State Zip", then the country.
export const addressLines = (address) => {
  if (!address) return [];
  const cityLine = [address.cityName, [address.stateName, address.postalCode].filter(Boolean).join(" ")]
    .filter(Boolean)
    .join(", ");
  return [address.addressLine1, address.addressLine2, cityLine, address.countryName].filter(Boolean);
};
