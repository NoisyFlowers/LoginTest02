CREATE EXTENSION postgis;
CREATE EXTENSION postgis_topology;

CREATE TABLE public.users (
	id serial PRIMARY KEY, 
	name text UNIQUE NOT NULL, 
	notes text
);
		
CREATE TABLE public.point_features (
	id serial PRIMARY KEY, 
	user_id integer REFERENCES public.users(id), 
	geom geometry, 
	preceded_by integer REFERENCES public.point_features(id), 
	superseded_by integer REFERENCES public.point_features(id), 
	removed boolean
);
		
CREATE TABLE public.line_features (
	id serial PRIMARY KEY, 
	user_id integer REFERENCES public.users(id), 
	geom geometry, 
	preceded_by integer REFERENCES public.line_features(id), 
	superseded_by integer REFERENCES public.line_features(id), 
	removed boolean
);

CREATE TABLE public.polygon_features (
	id serial PRIMARY KEY, 
	user_id integer REFERENCES public.users(id), 
	geom geometry, 
	preceded_by integer REFERENCES public.polygon_features(id), 
	superseded_by integer REFERENCES public.polygon_features(id), 
	removed boolean
);
		
CREATE TABLE public.feature_types (
	id serial PRIMARY KEY,
	user_id integer REFERENCES public.users(id),
	name text,
	notes text
);
		
CREATE TABLE public.attributes (
	id serial PRIMARY KEY,
	user_id integer REFERENCES public.users(id),
	feature_type_id integer REFERENCES public.feature_types(id),
	name text,
	data_type text, -- any constraints assumed for the field... for example should it only be integers only or is a long text string expected field
	notes text
);
		
CREATE TABLE public.possible_values (
	id serial PRIMARY KEY,
	attribute_id integer REFERENCES public.attributes(id) NOT NULL,
	user_id integer REFERENCES public.users(id),
	name text,
	definition text,
	notes text
);
		
CREATE TABLE public.point_value_links (
   id serial PRIMARY KEY,
   user_id integer REFERENCES public.users(id),
   feature_id integer REFERENCES public.point_features(id), 
   possible_value_id integer REFERENCES public.possible_values(id)
);

CREATE TABLE public.line_value_links (
   id serial PRIMARY KEY,
   user_id integer REFERENCES public.users(id),
   feature_id integer REFERENCES public.line_features(id), 
   possible_value_id integer REFERENCES public.possible_values(id)
);

CREATE TABLE public.polygon_value_links (
   id serial PRIMARY KEY,
   user_id integer REFERENCES public.users(id),
   feature_id integer REFERENCES public.polygon_features(id), 
   possible_value_id integer REFERENCES public.possible_values(id)
);