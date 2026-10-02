import { hearAboutUsApi } from "services/api";
import { useMasterNameOptions } from "composables/useMasterNameOptions";

// Hear About Us master as select options for a family's "How Did You Hear About Us?" (Family.Source is
// text). See useMasterNameOptions for ordering and how an already-saved value outside the master is kept.
export function useHearAboutUsOptions (getCurrentValues) {
  return useMasterNameOptions(hearAboutUsApi.list, getCurrentValues);
}
