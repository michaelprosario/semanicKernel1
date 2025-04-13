## What SQL do we need to create in the PG database for the vector data?

```
CREATE TABLE content_item_fragments (
    content_item_fragment_id UUID PRIMARY KEY,
    content_item_id UUID NOT NULL,
    embedding VECTOR(4),
    content TEXT NOT NULL,
    source TEXT NOT NULL
);
```


## What should be the size of the vector?

