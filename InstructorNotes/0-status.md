# Where are we?

We have a service that needs data from another service.

- We can ask that other service for it's data every time we need it.
    - the data is "fresh" - you are getting it from the source.
    - some other service "owns" that data, and we just need it as a "read model" to accomplish what we need.

    - Coupling - 
        - if that service is down, we are down.
        - If that service changes it's contract, ours will need to be updated as well.
        - Performance.

- We can ask that other service to just give us any updates.
    - Ok, we'll a copy of this. Just let us know (call our API) whenever there is a new title, or you remove one.

- We can share a database.
    - breaks independently deployable. If they change their database schema, we break, if we change it they break.
    