## What SQL do we need to create in the PG database for the vector data?

```
CREATE TABLE content_item_fragment (
    id TEXT PRIMARY KEY,
    content_item_id UUID NOT NULL,
    embedding VECTOR(1536),
    content TEXT NOT NULL,
    source TEXT NOT NULL
);
```


## What should be the size of the vector?

