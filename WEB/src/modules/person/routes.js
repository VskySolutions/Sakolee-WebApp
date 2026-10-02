export default [
  {
    path: "/persons",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "persons",
        component: () => import("modules/person/pages/index.vue"),
        meta: { requiresAuth: true, permissions: ["persons.read"], title: "Person" }
      },
      {
        path: ":id",
        name: "person_detail",
        component: () => import("modules/person/pages/detail.vue"),
        meta: { requiresAuth: true, permissions: ["persons.read"], title: "Person" }
      }
    ]
  }
];


// export default [
//   {
//     path: "/persons",
//     component: () => import("layouts/layout.vue"),
//     children: [
//       // Route for listing all persons (Index Page)
//       {
//         path: "",
//         name: "persons",
//         component: () => import("modules/person/pages/index.vue"),
//         meta: { requiresAuth: true, title: "Persons" }
//       },
//       // Route for viewing/editing detailed information of a specific person record
//       {
//         path: ":id",
//         name: "person_detail",
//         component: () => import("modules/person/components/view.vue"),
//         meta: { requiresAuth: true, title: "Person Details" }
//       }
//       // Note: Person creation is handled via a drawer modal in the index page, 
//       // so a separate create route is not required here.
//     ]
//   }
// ];