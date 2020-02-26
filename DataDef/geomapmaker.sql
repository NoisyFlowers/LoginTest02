CREATE TABLE public.users (
	id serial PRIMARY KEY, 
	name text UNIQUE NOT NULL, 
	notes text
);
		
CREATE TABLE public.features (
	id serial PRIMARY KEY, 
	user_id integer REFERENCES public.users(id), 
	geom geometry, 
	preceded_by integer REFERENCES public.features(id), 
	superseded_by integer REFERENCES public.features(id), 
	removed boolean
);
		
CREATE TABLE public.subclasses (
	id serial PRIMARY KEY,
	user_id integer REFERENCES public.users(id),
	name text,
	notes text
);
		
CREATE TABLE public.fields (
	id serial PRIMARY KEY,
	user_id integer REFERENCES public.users(id),
	subclass_id integer REFERENCES public.subclasses(id),
	name text,
	data_type text, -- any constraints assumed for the field... for example should it only be integers only or is a long text string expected field
	notes text
);
		
CREATE TABLE public.vocabularies (
	id serial PRIMARY KEY,
	field_id integer REFERENCES public.fields(id) NOT NULL,
	user_id integer REFERENCES public.users(id),
	name text,
	definition text,
	notes text
);
		
CREATE TABLE public.lookup (
   id serial PRIMARY KEY,
   user_id integer REFERENCES public.users(id),
   geom_id integer REFERENCES public.features(id), 
   vocab_id integer REFERENCES public.vocabularies(id)
);