# KitchenPC Sample Data

`KPCData.xml` is a static KitchenPC snapshot intended for examples and local experimentation. It
contains ingredient definitions and forms, NLP lookup data, and a small collection of recipes and
related records.

This data is intentionally limited. It does not represent the full KitchenPC PostgreSQL database
and should not be treated as a production dataset.

Where available, recipe records contain absolute URLs for their corresponding photographs on the
public `images.kitchenpc.com` CDN. The images are not bundled with this repository, so displaying
them requires an internet connection. Recipes without a production photograph have an empty image
URL and consuming samples should provide a fallback.
